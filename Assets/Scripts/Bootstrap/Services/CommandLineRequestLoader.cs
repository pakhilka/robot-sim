using System;
using System.IO;
using Newtonsoft.Json;
using RobotSim.Robot.Data.DTOs;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Loads level-run request JSON from CLI flag: -request <path>.
    /// </summary>
    public sealed class CommandLineRequestLoader
    {
        private const string RequestFlag = "-request";

        public bool ContainsRequestFlag(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], RequestFlag, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryLoadFromCommandLine(
            out LevelRunRequestDTO request,
            out string requestPath,
            out string error)
        {
            return TryLoad(
                Environment.GetCommandLineArgs(),
                out request,
                out requestPath,
                out error);
        }

        public bool TryLoad(
            string[] args,
            out LevelRunRequestDTO request,
            out string requestPath,
            out string error)
        {
            request = default;
            requestPath = string.Empty;
            error = string.Empty;

            if (!TryParseRequestPath(args, out requestPath, out error))
            {
                return false;
            }

            return TryLoadFromPath(
                requestPath,
                out request,
                out requestPath,
                out error);
        }

        public bool TryLoadFromPath(
            string inputRequestPath,
            out LevelRunRequestDTO request,
            out string fullRequestPath,
            out string error)
        {
            request = default;
            fullRequestPath = string.Empty;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(inputRequestPath))
            {
                error = "Request path is empty.";
                return false;
            }

            string fullPath = Path.GetFullPath(inputRequestPath);
            if (!File.Exists(fullPath))
            {
                error = $"Request file does not exist: {fullPath}";
                return false;
            }

            string json;
            try
            {
                json = File.ReadAllText(fullPath);
            }
            catch (Exception ex)
            {
                error = $"Failed to read request file '{fullPath}'. {ex.Message}";
                return false;
            }

            if (!TryLoadFromJson(json, out request, out error))
            {
                error = $"Invalid request JSON at '{fullPath}'. {error}";
                return false;
            }

            fullRequestPath = fullPath;
            return true;
        }

        public bool TryLoadFromJson(
            string json,
            out LevelRunRequestDTO request,
            out string error)
        {
            request = default;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Request JSON is empty.";
                return false;
            }

            try
            {
                request = JsonConvert.DeserializeObject<LevelRunRequestDTO>(json);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static bool TryParseRequestPath(string[] args, out string requestPath, out string error)
        {
            requestPath = string.Empty;
            error = string.Empty;

            if (args == null || args.Length == 0)
            {
                error = "Missing CLI argument '-request <path>'.";
                return false;
            }

            int flagIndex = -1;
            int flagCount = 0;

            for (int i = 0; i < args.Length; i++)
            {
                if (!string.Equals(args[i], RequestFlag, StringComparison.Ordinal))
                {
                    continue;
                }

                flagCount++;
                if (flagIndex < 0)
                {
                    flagIndex = i;
                }
            }

            if (flagCount == 0)
            {
                error = "Missing CLI argument '-request <path>'.";
                return false;
            }

            if (flagCount > 1)
            {
                error = "CLI argument '-request' must be provided once.";
                return false;
            }

            int pathIndex = flagIndex + 1;
            if (pathIndex >= args.Length || string.IsNullOrWhiteSpace(args[pathIndex]))
            {
                error = "Missing path value after '-request'.";
                return false;
            }

            requestPath = args[pathIndex].Trim();
            return true;
        }
    }
}
