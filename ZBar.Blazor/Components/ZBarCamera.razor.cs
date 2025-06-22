using Microsoft.AspNetCore.Components;
using ZBar.Blazor.Config;
using ZBar.Blazor.Dtos;
using ZBar.Blazor.Interop;

namespace ZBar.Blazor.Components
{
    /// <summary>
    /// A component which reads a video stream from any of the device's available video input
    /// sources and continually scans the incoming video frames for barcode information.
    /// </summary>
    partial class ZBarCamera : IAsyncDisposable
    {
        [Inject] CameraInterop CameraInterop { get; set; }

        /// <summary>
        /// The rate in milliseconds at which the camera image data will be polled to process for bar code information.
        /// Lower values increase responsiveness at the expense of requiring more processing power.
        /// </summary>
        /// <remarks>
        /// Defaults to 1000.
        /// </remarks>
        [Parameter] public int ScanInterval { get; set; } = 1000;

        /// <summary>
        /// Specifies the type of video output displayed from this camera.
        /// </summary>
        /// <remarks>
        /// Defaults to CameraViewType.ScannerFeed.
        /// </remarks>
        [Parameter] public CameraViewType CameraViewType { get; set; } = CameraViewType.ScannerFeed;

        [Parameter] public int Width { get; set; } = 1280;
        [Parameter] public int Height { get; set; } = 720;

        private ElementReference Video;
        private ElementReference Canvas;

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        /// <summary>
        /// Returns a list of camera devices that are currently available.
        /// </summary>
        /// <remarks>
        /// Calling this function will request camera permissions from the user if not already approved.
        /// </remarks>
        public async Task<HardwareDevice[]> GetAvailableDevices()
        {
            return await CameraInterop.GetAvailableCameras();
        }

        /// <summary>
        /// Starts requested camera and begins scanning for barcode information.
        /// </summary>
        /// <param name="hardwareDeviceId">
        /// Specifies the hardware device to use for video input. If not provided, the system default will be used.
        /// </param>
        /// <param name="verbose">
        /// Enables verbose browser console output for scanner options configuration. Useful for debugging purposes.
        /// </param>
        /// <remarks>
        /// Calling this function will request camera permissions from the user if not already approved.
        /// </remarks>
        public async Task StartVideoFeed(string hardwareDeviceId = null, bool verbose = false)
        {
            await CameraInterop.StartVideoFeed(Scanner.Interop, Video, Canvas, hardwareDeviceId, ScanInterval, ScannerOptions, verbose);
        }

        /// <summary>
        /// Stops the camera from recording and related barcode scanning processes.
        /// </summary>
        public async Task EndVideoFeed()
        {
            await CameraInterop.EndVideoFeed(Video);
        }

        public async ValueTask DisposeAsync()
        {
            await CameraInterop.EndVideoFeed(Video);
        }

        private string GetContainerDisplay()
        {
            if (CameraViewType == CameraViewType.None)
                return "display: none;";
            else return "display: flex;";
        }

        private string GetVideoDisplay()
        {
            if (CameraViewType == CameraViewType.VideoFeed)
                return "display: block;";
            else return "display: none;";
        }

        private string GetCanvasDisplay()
        {
            if (CameraViewType == CameraViewType.ScannerFeed)
                return "display: block;";
            else return "display: none;";
        }
    }
}