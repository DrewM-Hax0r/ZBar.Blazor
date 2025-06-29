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
        /// <summary>
        /// This value is applicable only when AutoScan is enabled.
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

        /// <summary>
        /// When enabled, additional information will be printed to the browser console at key lifecycle events for ZBarCamera functionality. Useful for debugging purposes.
        /// </summary>
        /// <remarks>
        /// Defaults to false.
        /// </remarks>
        [Parameter] public bool Verbose { get; set; } = false;

        private CameraInterop CameraInterop;
        private ElementReference Video;
        private ElementReference Canvas;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            CameraInterop = new CameraInterop(JsRuntime); // TODO: [.NET 10] Switch to constructor injection
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            bool? updatedAutoScan = null;
            int? updatedScanInterval = null;
            bool? updatedVerbose = null;

            if (parameters.TryGetValue<bool>(nameof(AutoScan), out var autoScan) && autoScan != AutoScan)
            {
                updatedAutoScan = autoScan;
            }

            if (parameters.TryGetValue<int>(nameof(ScanInterval), out var scanInterval) && scanInterval != ScanInterval)
            {
                updatedScanInterval = scanInterval;
            }

            if (parameters.TryGetValue<bool>(nameof(Verbose), out var verbose) && verbose != Verbose)
            {
                updatedVerbose = verbose;
            }

            await base.SetParametersAsync(parameters);

            // Call async methods after all reads to ParameterView have completed to avoid stale ParameterView context
            await UpdateJsConfiguration(updatedAutoScan, updatedScanInterval, updatedVerbose);
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
        /// Starts the requested camera's video feed.
        /// </summary>
        /// <param name="hardwareDeviceId">
        /// Specifies the hardware device to use for video input. If not provided, the system default will be used.
        /// </param>
        /// <remarks>
        /// Calling this function will request camera permissions from the user if not already approved.
        /// </remarks>
        public async Task StartVideoFeed(string hardwareDeviceId = null)
        {
            await CameraInterop.StartVideoFeed(ScannerInterop.Interop, Video, Canvas, hardwareDeviceId, ScanInterval, ScannerOptions, Verbose);
        }

        /// <summary>
        /// Stops the current camera's video feed.
        /// </summary>
        public async Task EndVideoFeed()
        {
            await CameraInterop.EndVideoFeed(Video);
        }

        /// <summary>
        /// Performs a single scan on the currently active video feed.
        /// Scan results will be provided to the <see cref="ZBarComponent.OnBarcodesFound" /> and <see cref="ZBarComponent.OnBarcodesNotFound" /> event callbacks.
        /// </summary>
        /// <remarks>
        /// Calling this method has no effect if the video feed has not been started or if <see cref="ZBarComponent.AutoScan" /> is enabled.
        /// </remarks>
        public async Task Scan()
        {
            if (!AutoScan)
            {
                await CameraInterop.ScanOnce(ScannerInterop.Interop, Video, Canvas);
            }
        }

        private async Task UpdateJsConfiguration(bool? autoScan, int? scanInterval, bool? verbose)
        {
            if (autoScan.HasValue)
            {
                if (autoScan.Value) await CameraInterop.EnableAutoScan(ScannerInterop.Interop, Video, Canvas, ScanInterval, Verbose);
                else await CameraInterop.DisableAutoScan(Video);
            }

            if (scanInterval.HasValue)
            {
                await CameraInterop.UpdateScanInterval(ScannerInterop.Interop, Video, Canvas, scanInterval.Value);
            }

            if (verbose.HasValue)
            {
                await CameraInterop.UpdateVerbosity(Video, verbose.Value);
            }
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

        public async ValueTask DisposeAsync()
        {
            await this.EndVideoFeed();
            await CameraInterop.DisposeAsync();
        }
    }
}