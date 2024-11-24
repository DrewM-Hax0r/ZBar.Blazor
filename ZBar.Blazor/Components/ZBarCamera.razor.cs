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

        /// <summary>
        /// Specify one or more barcode types to scan for. Multiple types can be combined as flags.
        /// Setting only the barcode types applicable to your workflow can improve performance.
        /// </summary>
        /// <remarks>
        /// Defaults to all supported barcode types.
        /// </remarks>
        [Parameter] public BarcodeType ScanFor { get; set; }

        /// <summary>
        /// Only applicable to barcode types that are variable in length.
        /// Barcodes with values smaller in length than the specified number of characters will not be reported.
        /// Setting to 0 will disable the minimum value check.
        /// </summary>
        /// <remarks>
        /// Defaults to 0.
        /// </remarks>
        [Parameter] public int MinimumValueLength { get; set; }

        /// <summary>
        /// Only applicable to barcode types that are variable in length.
        /// Barcodes with values greater in length than the specified number of characters will not be reported.
        /// Setting to 0 will disable the maximum value check.
        /// </summary>
        /// <remarks>
        /// Defaults to 0.
        /// </remarks>
        [Parameter] public int MaximumValueLength { get; set; }

        /// <summary>
        /// Only applicable to barcode types that can contain non-numeric characters.
        /// Enable to support the full set of ASCII characters.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        [Parameter] public bool EnableFullCharacterSet { get; set; }

        [Parameter] public int Width { get; set; } = 1280;
        [Parameter] public int Height { get; set; } = 720;

        private readonly ScannerOptions ScannerOptions;

        private DotNetObjectReference<ZBarCamera> Camera;
        private ElementReference Video;
        private ElementReference Canvas;

        public ZBarCamera()
        {
            ScannerOptions = new();
            ScanFor = ScannerOptions.ScanFor;
            MinimumValueLength = ScannerOptions.MinimumValueLength;
            MaximumValueLength = ScannerOptions.MaximumValueLength;
            EnableFullCharacterSet = ScannerOptions.EnableFullCharacterSet;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Camera = DotNetObjectReference.Create(this);
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);

            if (parameters.TryGetValue<BarcodeType>(nameof(ScanFor), out var scanFor))
            {
                ScannerOptions.ScanFor = scanFor;
            }

            if (parameters.TryGetValue<int>(nameof(MinimumValueLength), out var minimumValueLength))
            {
                ScannerOptions.MinimumValueLength = minimumValueLength;
            }

            if (parameters.TryGetValue<int>(nameof(MaximumValueLength), out var maximumValueLength))
            {
                ScannerOptions.MaximumValueLength = maximumValueLength;
            }

            if (parameters.TryGetValue<bool>(nameof(EnableFullCharacterSet), out var enableFullCharacterSet))
            {
                ScannerOptions.EnableFullCharacterSet = enableFullCharacterSet;
            }
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