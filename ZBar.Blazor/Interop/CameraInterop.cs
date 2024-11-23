using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ZBar.Blazor.Interop
{
    internal class CameraInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public CameraInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/ZBar.Blazor/camera.js").AsTask());
        }

        public async Task StartVideoFeed(ElementReference video, ElementReference canvas, int scanInterval)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("startVideoFeed", video, canvas, scanInterval);
        }

        public async Task EndVideoFeed(ElementReference video)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("endVideoFeed", video);
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