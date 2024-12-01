using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ZBar.Blazor.Config;
using ZBar.Blazor.Dtos;

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

        public async Task<HardwareDevice[]> GetAvailableCameras()
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<HardwareDevice[]>("getAvailableCameras");
        }

        public async Task StartVideoFeed(ElementReference video, ElementReference canvas, string deviceId, int scanInterval, ScannerOptions scannerOptions, bool verbose)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("startVideoFeed", video, canvas, deviceId, scanInterval, scannerOptions.Export(), verbose);
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