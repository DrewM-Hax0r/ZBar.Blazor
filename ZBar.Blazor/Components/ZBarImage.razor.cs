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
        /// Specifies how image data will be provided to this component. Defaults to Stream.
        /// </summary>
        /// <remarks>
        /// When the SourceType is set to Stream, image data must be loaded using the LoadFromStreamAsync method.
        /// When the SourceType is set to Url, image data must be loaded using the Src component parameter.
        /// </remarks>
        [Parameter] public ImageSourceType SourceType { get; set; } = ImageSourceType.Stream;

        /// <summary>
        /// Specifies the type of image output displayed from this component.
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

        /// <summary>
        /// Set this to a URL that points to a valid image to load that image. The image will be scanned immediately if AutoScan is enabled.
        /// </summary>
        [Parameter] public string Src { get; set; }

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
            bool wasSrcUpdated = false;
            string updatedSrc = null;

            ImageSourceType? updatedSourceType = null;
            bool? updatedVerbose = null;

            if (parameters.TryGetValue<string>(nameof(Src), out var src) && src != Src)
            {
                wasSrcUpdated = true;
                updatedSrc = src;
            }

            if (parameters.TryGetValue<ImageSourceType>(nameof(SourceType), out var sourceType) && sourceType != SourceType)
            {
                updatedSourceType = sourceType;
            }

            if (parameters.TryGetValue<bool>(nameof(Verbose), out var verbose) && verbose != Verbose)
            {
                updatedVerbose = verbose;
            }

            await base.SetParametersAsync(parameters);

            // Call async methods after all reads to ParameterView have completed to avoid stale ParameterView context
            await UpdateJsConfiguration(updatedSourceType, updatedVerbose);

            var newSrcSet = wasSrcUpdated && SourceType == ImageSourceType.Url;
            var reloadExistingSrc = updatedSourceType.HasValue && updatedSourceType.Value == ImageSourceType.Url;
            if (newSrcSet || reloadExistingSrc)
            {
                var srcToUse = Src?.Trim();
                if (!string.IsNullOrWhiteSpace(srcToUse))
                {
                    await ImageInterop.LoadFromUrlAsync(srcToUse, Canvas, ScannerOptions.Export(), Verbose);
                }
            }
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
            if (SourceType == ImageSourceType.Stream)
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

        private async Task UpdateJsConfiguration(ImageSourceType? sourceType, bool? verbose)
        {
            if (sourceType.HasValue)
            {
                await ImageInterop.ClearCanvas(Canvas);
            }

            if (verbose.HasValue)
            {
                await ImageInterop.UpdateVerbosity(Canvas, verbose.Value);
            }
        }

        public override void Dispose()
        {
            ImageInterop?.Dispose();
            base.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            await ImageInterop.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}