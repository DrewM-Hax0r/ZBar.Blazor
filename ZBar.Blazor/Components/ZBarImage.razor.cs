using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ZBar.Blazor.Config;
using ZBar.Blazor.Interop;

namespace ZBar.Blazor.Components
{
    /// <summary>
    /// A component which scans provided image data for barcode information.
    /// </summary>
    partial class ZBarImage : IDisposable, IAsyncDisposable
    {
        [Inject] private IJSRuntime JsRuntime { get; set; }

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

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ImageInterop = new ImageInterop(JsRuntime, this); // TODO: [.NET 10] Switch to constructor injection
        }

        public async Task LoadFromStreamAsync(Stream stream)
        {
            await ImageInterop.LoadFromStreamAsync(Canvas, stream);
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

        public override void Dispose()
        {
            base.Dispose();
            ImageInterop?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await ImageInterop.DisposeAsync();
        }
    }
}