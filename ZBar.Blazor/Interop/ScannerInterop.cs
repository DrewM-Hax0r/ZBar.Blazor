using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;
using ZBar.Blazor.Components;
using ZBar.Blazor.Config;
using ZBar.Blazor.Dtos;
using static ZBar.Blazor.Config.ScannerOptions;

namespace ZBar.Blazor.Interop
{
    internal class ScannerInterop : IDisposable, IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> ModuleTask;
        private readonly ZBarComponent Component;

        internal readonly DotNetObjectReference<ScannerInterop> Interop;

        [DynamicDependency(nameof(OnAfterScan))]
        public ScannerInterop(IJSRuntime jsRuntime, ZBarComponent component)
        {
            Component = component;
            Interop = DotNetObjectReference.Create(this);
            ModuleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/ZBar.Blazor/scanner.js").AsTask());
        }

        public async Task UpdateScannerConfig(ElementReference key, SymbolOption[] scannerOptions, bool verbose)
        {
            var module = await ModuleTask.Value;
            await module.InvokeVoidAsync("updateScannerConfig", key, scannerOptions, verbose);
        }

        [JSInvokable]
        public void OnAfterScan(Symbol[] symbols)
        {
            if (symbols.Length > 0)
            {
                var scanResult = BuildScanResult(symbols);
                Component.OnBarcodesFound.InvokeAsync(scanResult);
            }
            else Component.OnBarcodesNotFound.InvokeAsync();
        }

        private ScanResult BuildScanResult(Symbol[] symbols)
        {
            var barcodes = symbols.Select(s => new Barcode() {
                Type = s.TypeName.ToBarcodeType(),
                Value = s.RawValue
            }).ToArray();

            return new ScanResult(barcodes);
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