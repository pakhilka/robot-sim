namespace RobotSim.Bootstrap.Data
{
    /// <summary>
    /// Input options for request loading stage.
    /// </summary>
    public readonly struct RequestLoadingOptions
    {
        public RequestLoadingOptions(
            bool useEditorRequestFallback,
            string editorRequestPath)
        {
            UseEditorRequestFallback = useEditorRequestFallback;
            EditorRequestPath = editorRequestPath ?? string.Empty;
        }

        public bool UseEditorRequestFallback { get; }
        public string EditorRequestPath { get; }
    }
}
