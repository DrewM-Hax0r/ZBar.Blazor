namespace ZBar.Blazor.Config
{
    public enum CameraViewType
    {
        /// <summary>
        /// Specifies that no video output is displayed.
        /// </summary>
        None,

        /// <summary>
        /// Specifies the displayed video output to be the direct feed from the video source.
        /// </summary>
        VideoFeed,

        /// <summary>
        /// Specifies the displayed video output to be the individual video frames captured at each scan interval.
        /// </summary>
        ScannerFeed
    }
}