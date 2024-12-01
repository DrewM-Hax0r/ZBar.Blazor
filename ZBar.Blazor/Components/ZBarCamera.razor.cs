using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ZBar.Blazor.Config;
using ZBar.Blazor.Dtos;
using ZBar.Blazor.Interop;

namespace ZBar.Blazor.Components
{
    partial class ZBarCamera : IDisposable, IAsyncDisposable
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

        [Parameter] public int Width { get; set; } = 1280;
        [Parameter] public int Height { get; set; } = 720;

        private DotNetObjectReference<ZBarCamera> Camera;
        private ElementReference Video;
        private ElementReference Canvas;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Camera = DotNetObjectReference.Create(this);
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
        /// <remarks>
        /// Calling this function will request camera permissions from the user if not already approved.
        /// </remarks>
        public async Task StartVideoFeed(string hardwareDeviceId = null)
        {
            await CameraInterop.StartVideoFeed(Video, Canvas, hardwareDeviceId, ScanInterval, ScannerOptions);
        }

        /// <summary>
        /// Stops the camera from recording and related barcode scanning processes.
        /// </summary>
        public async Task EndVideoFeed()
        {
            await CameraInterop.EndVideoFeed(Video);
        }

        public void Dispose()
        {
            Camera?.Dispose();
            Camera = null;
        }

        public async ValueTask DisposeAsync()
        {
            await CameraInterop.EndVideoFeed(Video);
        }
    }
}