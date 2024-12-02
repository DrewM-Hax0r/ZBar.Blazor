using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;
using ZBar.Blazor.Components;
using ZBar.Blazor.Config;
using ZBar.Blazor.Dtos;

namespace ZBar.Blazor.Interop
{
    internal class ScannerInterop : IDisposable
    {
        private readonly ZBarComponent Component;
        internal readonly DotNetObjectReference<ScannerInterop> Interop;

        [DynamicDependency(nameof(OnAfterScan))]
        public ScannerInterop(ZBarComponent component)
        {
            Component = component;
            Interop = DotNetObjectReference.Create(this);
        }

        [JSInvokable]
        public void OnAfterScan(Symbol[] symbols)
        {
            if (symbols.Length > 0)
            {
                Component.OnBarcodesFound?.Invoke(symbols.Select(s => new Barcode()
                {
                    Type = s.TypeName.ToBarcodeType(),
                    Value = s.RawValue
                }).ToArray());
            }
            else
            {
                Component.OnBarcodesNotFound?.Invoke();
            }
        }

        public void Dispose()
        {
            Interop?.Dispose();
        }
    }
}