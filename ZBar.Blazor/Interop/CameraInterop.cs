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

        public async Task StartVideoFeed(DotNetObjectReference<ScannerInterop> interop, ElementReference video, ElementReference canvas, string deviceId, int scanInterval, ScannerOptions scannerOptions, bool verbose)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("startVideoFeed", interop, video, canvas, deviceId, scannerOptions.AutoScan, scanInterval, scannerOptions.Export(), verbose);
        }

        public async Task EndVideoFeed(ElementReference video)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("endVideoFeed", video);
        }

        public async Task SetVerbosity(ElementReference video, bool value)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("setVerbosity", video, value);
        }

        public async Task ScanOnce(DotNetObjectReference<ScannerInterop> interop, ElementReference video, ElementReference canvas)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("scanOnce", interop, video, canvas);
        }

        public async Task EnableAutoScan(DotNetObjectReference<ScannerInterop> interop, ElementReference video, ElementReference canvas, int scanInterval, bool verbose)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("enableAutoScan", interop, video, canvas, scanInterval, verbose);
        }

        public async Task DisableAutoScan(ElementReference video)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("disableAutoScan", video);
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