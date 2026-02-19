using System;
using RobotSim.Bootstrap.Data;
using UnityEngine;

namespace RobotSim.Bootstrap.Services
{
    /// <summary>
    /// Resolves request source precedence and loads LevelRunRequest DTO.
    /// </summary>
    public sealed class RequestLoadingService
    {
        private readonly CommandLineRequestLoader _loader;

        public RequestLoadingService()
            : this(new CommandLineRequestLoader())
        {
        }

        public RequestLoadingService(CommandLineRequestLoader loader)
        {
            _loader = loader ?? new CommandLineRequestLoader();
        }

        public RequestLoadResult Load(RequestLoadingOptions options)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (_loader.ContainsRequestFlag(args))
            {
                if (_loader.TryLoad(args, out var request, out string requestPath, out string error))
                {
                    return RequestLoadResult.Success(request, requestPath, RequestSource.CommandLinePath);
                }

                return RequestLoadResult.Fail(RequestSource.CommandLinePath, error);
            }

            if (Application.isEditor && options.UseEditorRequestFallback)
            {
                string editorRequestPath = options.EditorRequestPath?.Trim() ?? string.Empty;
                bool hasEditorPathInput = !string.IsNullOrWhiteSpace(editorRequestPath);
                string editorPathError = string.Empty;

                if (hasEditorPathInput &&
                    _loader.TryLoadFromPath(editorRequestPath, out var editorRequest, out string fullRequestPath, out editorPathError))
                {
                    return RequestLoadResult.Success(editorRequest, fullRequestPath, RequestSource.EditorPath);
                }

                string error = BuildEditorFallbackError(hasEditorPathInput, editorPathError);
                return RequestLoadResult.Fail(RequestSource.EditorPath, error);
            }

            if (_loader.TryLoad(args, out var defaultRequest, out string defaultRequestPath, out string defaultError))
            {
                return RequestLoadResult.Success(defaultRequest, defaultRequestPath, RequestSource.CommandLinePath);
            }

            return RequestLoadResult.Fail(RequestSource.None, defaultError);
        }

        private static string BuildEditorFallbackError(
            bool hasPathInput,
            string editorPathError)
        {
            if (!hasPathInput)
            {
                return "Missing request input. Provide CLI '-request <path>' or configure editor fallback request input.";
            }

            string normalizedPathError = string.IsNullOrWhiteSpace(editorPathError)
                ? "request file is missing, unreadable, or invalid JSON."
                : editorPathError;
            return $"Editor fallback request path is invalid. {normalizedPathError}";
        }
    }
}
