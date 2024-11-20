using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ZBar.Blazor
{
    public class CameraInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public CameraInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/ZBar.Blazor/camera.js").AsTask());
        }

        public async Task StartVideoFeed(ElementReference video)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("window.zBarBlazor.camera.startVideoFeed", video);
        }

        public async Task EndVideoFeed(ElementReference video)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("window.zBarBlazor.camera.endVideoFeed", video);
        }

        public async ValueTask<byte[]> CaptureImage(ElementReference video, ElementReference canvas, int imageWidth, int imageHeight)
        {
            var module = await moduleTask.Value;
            var base64Image = await module.InvokeAsync<string>("window.zBarBlazor.camera.captureImage", video, canvas, imageWidth, imageHeight);
            return Convert.FromBase64String(base64Image);
        }

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}