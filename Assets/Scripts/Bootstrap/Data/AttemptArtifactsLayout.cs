namespace RobotSim.Bootstrap.Data
{
    /// <summary>
    /// Runtime artifact paths for a single attempt.
    /// </summary>
    public readonly struct AttemptArtifactsLayout
    {
        public AttemptArtifactsLayout(
            string projectRootPath,
            string artifactsRootPath,
            string attemptFolderPath,
            string requestPath,
            string resultPath,
            string videoPath,
            string framesPath)
        {
            ProjectRootPath = projectRootPath;
            ArtifactsRootPath = artifactsRootPath;
            AttemptFolderPath = attemptFolderPath;
            RequestPath = requestPath;
            ResultPath = resultPath;
            VideoPath = videoPath;
            FramesPath = framesPath;
        }

        public string ProjectRootPath { get; }
        public string ArtifactsRootPath { get; }
        public string AttemptFolderPath { get; }
        public string RequestPath { get; }
        public string ResultPath { get; }
        public string VideoPath { get; }
        public string FramesPath { get; }
    }
}
