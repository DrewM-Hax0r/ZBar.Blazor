using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;
using ZBar.Blazor.Components;

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

        public async Task LoadFromStreamAsync(ElementReference canvas, Stream stream)
        {
            using var jsStream = new DotNetStreamReference(stream, leaveOpen: true);
            var module = await ModuleTask.Value;
            await module.InvokeVoidAsync("loadFromStream", Interop, canvas, jsStream);
        }

        [JSInvokable]
        public void OnImageLoadSuccess()
        {
            Component.OnImageLoadSuccess.InvokeAsync();
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