using Microsoft.AspNetCore.Components;
using ZBar.Blazor.Config;
using ZBar.Blazor.Interop;

namespace ZBar.Blazor.Components
{
    /// <summary>
    /// A component which scans provided image data for barcode information.
    /// </summary>
    partial class ZBarImage : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Specifies the type of image output displayed from this camera.
        /// </summary>
        /// <remarks>
        /// Defaults to ImageViewType.ImageFeed.
        /// </remarks>
        [Parameter] public ImageViewType ImageViewType { get; set; } = ImageViewType.ImageFeed;

        /// <summary>
        /// Binds a callback function that will be executed when a provided image source is loaded without issue.
        /// </summary>
        [Parameter] public EventCallback OnImageLoadSuccess { get; set; }

        /// <summary>
        /// Binds a callback function that will be executed when a provided image source could not be loaded or interpreted as image data.
        /// </summary>
        [Parameter] public EventCallback OnImageLoadFailure { get; set; }

        private ImageInterop ImageInterop;
        private ElementReference Canvas;

        protected override ElementReference ScannerKey => Canvas;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ImageInterop = new ImageInterop(JsRuntime, this); // TODO: [.NET 10] Switch to constructor injection
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            bool? updatedVerbose = null;

            if (parameters.TryGetValue<bool>(nameof(Verbose), out var verbose) && verbose != Verbose)
            {
                updatedVerbose = verbose;
            }

            await base.SetParametersAsync(parameters);

            // Call async methods after all reads to ParameterView have completed to avoid stale ParameterView context
            await UpdateJsConfiguration(updatedVerbose);
        }

        internal async Task ImageLoadSuccess()
        {
            await OnImageLoadSuccess.InvokeAsync();
            if (AutoScan) await Scan();
        }

        /// <summary>
        /// Loads image data from a stream and draws it in the canvas.
        /// </summary>
        public async Task LoadFromStreamAsync(Stream stream)
        {
            await ImageInterop.LoadFromStreamAsync(stream, Canvas, ScannerOptions.Export(), Verbose);
        }

        /// <summary>
        /// Performs a single scan on the image currently drawn in the canvas.
        /// Scan results will be provided to the <see cref="ZBarComponent.OnBarcodesFound" /> and <see cref="ZBarComponent.OnBarcodesNotFound" /> event callbacks.
        /// </summary>
        public async Task Scan()
        {
            await ImageInterop.ScanImageAsync(ScannerInterop.Interop, Canvas);
        }

        private string GetContainerDisplay()
        {
            if (ImageViewType == ImageViewType.None)
                return "display: none;";
            else return "display: flex;";
        }

        private string GetCanvasDisplay()
        {
            if (ImageViewType == ImageViewType.ImageFeed)
                return "display: block;";
            else return "display: none;";
        }

        private async Task UpdateJsConfiguration(bool? verbose)
        {
            if (verbose.HasValue)
            {
                await ImageInterop.UpdateVerbosity(Canvas, verbose.Value);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            ImageInterop?.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            await ImageInterop.DisposeAsync();
        }
    }
}