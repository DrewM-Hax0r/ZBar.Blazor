using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ZBar.Blazor.Interop;

namespace ZBar.Blazor
{
    partial class ZBarCamera : IDisposable, IAsyncDisposable
    {
        [Inject] CameraInterop CameraInterop { get; set; }

        /// <summary>
        /// The rate in milliseconds at which the camera image data will be pulled to process for bar code information.
        /// Lower values increase responsiveness at the expense of requiring more processing power.
        /// </summary>
        /// <remarks>Defaults to 1000.</remarks>
        [Parameter] public int ScanInterval { get; set; } = 1000;

        [Parameter] public int Width { get; set; } = 320;
        [Parameter] public int Height { get; set; } = 240;

        private DotNetObjectReference<ZBarCamera> Camera;
        private ElementReference Video;
        private ElementReference Canvas;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Camera = DotNetObjectReference.Create(this);
        }

        private async Task StartVideoFeed()
        {
            await CameraInterop.StartVideoFeed(Video, Canvas, ScanInterval);
        }

        private async Task EndVideoFeed()
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