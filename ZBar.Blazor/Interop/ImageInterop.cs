using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;
using ZBar.Blazor.Components;
using ZBar.Blazor.Config;

namespace ZBar.Blazor.Interop
{
    internal class ImageInterop : IDisposable, IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> ModuleTask;
        private readonly ZBarImage Component;

        internal readonly DotNetObjectReference<ImageInterop> Interop;

        [DynamicDependency(nameof(OnImageLoadSuccess))]
        [DynamicDependency(nameof(OnImageLoadFailure))]
        public ImageInterop(IJSRuntime jsRuntime, ZBarImage component)
        {
            Component = component;
            Interop = DotNetObjectReference.Create(this);
            ModuleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/ZBar.Blazor/image.js").AsTask());
        }

        public async Task LoadFromStreamAsync(Stream stream, ElementReference canvas, ScannerOptions.SymbolOption[] scannerOptions, bool verbose)
        {
            using var jsStream = new DotNetStreamReference(stream, leaveOpen: true);
            var module = await ModuleTask.Value;
            await module.InvokeVoidAsync("loadFromStream", Interop, jsStream, canvas, scannerOptions, verbose);
        }

        public async Task LoadFromUrlAsync(string src, ElementReference canvas, ScannerOptions.SymbolOption[] scannerOptions, bool verbose)
        {
            var module = await ModuleTask.Value;
            await module.InvokeVoidAsync("loadFromUrl", Interop, src, canvas, scannerOptions, verbose);
        }

        public async Task ScanImageAsync(DotNetObjectReference<ScannerInterop> scannerInterop, ElementReference canvas)
        {
            var module = await ModuleTask.Value;
            await module.InvokeVoidAsync("scanImage", scannerInterop, canvas);
        }

        public async Task ClearCanvas(ElementReference canvas)
        {
            var module = await ModuleTask.Value;
            await module.InvokeVoidAsync("clearCanvas", canvas);
        }

        public async Task UpdateVerbosity(ElementReference canvas, bool value)
        {
            var module = await ModuleTask.Value;
            await module.InvokeVoidAsync("updateVerbosity", canvas, value);
        }        

        [JSInvokable]
        public async Task OnImageLoadSuccess()
        {
            await Component.ImageLoadSuccess();
        }

        [JSInvokable]
        public void OnImageLoadFailure()
        {
            Component.OnImageLoadFailure.InvokeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (ModuleTask.IsValueCreated)
            {
                var module = await ModuleTask.Value;
                await module.DisposeAsync();
            }
        }

        public void Dispose()
        {
            Interop?.Dispose();
        }
    }
}