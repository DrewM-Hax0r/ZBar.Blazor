using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ZBar.Blazor.Config;
using ZBar.Blazor.Dtos;

namespace ZBar.Blazor.Interop
{
    internal class CameraInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> ModuleTask;

        public CameraInterop(IJSRuntime jsRuntime)
        {
            ModuleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/ZBar.Blazor/camera.js").AsTask());
        }

        public async Task<HardwareDevice[]> GetAvailableCameras()
        {
            var module = await ModuleTask.Value;
            return await module.InvokeAsync<HardwareDevice[]>("getAvailableCameras");
        }

        public async Task StartVideoFeed(DotNetObjectReference<ScannerInterop> interop, ElementReference video, ElementReference canvas, string deviceId, bool autoScan, int scanInterval, ScannerOptions scannerOptions, bool verbose)
        {
            var module = await ModuleTask.Value;
            await module.InvokeVoidAsync("startVideoFeed", interop, video, canvas, deviceId, autoScan, scanInterval, scannerOptions.Export(), verbose);
        }

        public async Task EndVideoFeed(ElementReference video)
        {
            var module = await ModuleTask.Value;
            await module.InvokeVoidAsync("endVideoFeed", video);
        }

        public async Task ScanOnce(DotNetObjectReference<ScannerInterop> interop, ElementReference video, ElementReference canvas)
        {
            var module = await ModuleTask.Value;
            await module.InvokeVoidAsync("scanOnce", interop, video, canvas);
        }

        public async Task EnableAutoScan(DotNetObjectReference<ScannerInterop> interop, ElementReference video, ElementReference canvas, int scanInterval, bool verbose)
        {
            var module = await ModuleTask.Value;
            await module.InvokeVoidAsync("enableAutoScan", interop, video, canvas, scanInterval, verbose);
        }

        public async Task DisableAutoScan(ElementReference video)
        {
            var module = await ModuleTask.Value;
            await module.InvokeVoidAsync("disableAutoScan", video);
        }

        public async Task UpdateScanInterval(DotNetObjectReference<ScannerInterop> interop, ElementReference video, ElementReference canvas, int scanInterval)
        {
            var module = await ModuleTask.Value;
            await module.InvokeVoidAsync("updateScanInterval", interop, video, canvas, scanInterval);
        }

        public async Task UpdateVerbosity(ElementReference video, bool value)
        {
            var module = await ModuleTask.Value;
            await module.InvokeVoidAsync("updateVerbosity", video, value);
        }

        public async ValueTask DisposeAsync()
        {
            if (ModuleTask.IsValueCreated)
            {
                var module = await ModuleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}