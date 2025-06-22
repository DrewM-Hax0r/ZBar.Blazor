using Microsoft.JSInterop;

namespace ZBar.Blazor.Interop
{
    internal class ImageInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public ImageInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/ZBar.Blazor/image.js").AsTask());
        }

        public async Task LoadFromStreamAsync(Stream stream)
        {
            using var jsStream = new DotNetStreamReference(stream, leaveOpen: true);
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("loadFromStream", jsStream);
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