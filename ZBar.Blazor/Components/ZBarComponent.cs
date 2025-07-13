using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ZBar.Blazor.Config;
using ZBar.Blazor.Dtos;
using ZBar.Blazor.Interop;
using static ZBar.Blazor.Config.ScannerOptions;

namespace ZBar.Blazor.Components
{
    public abstract class ZBarComponent : ComponentBase, IDisposable, IAsyncDisposable
    {
        [Inject] protected IJSRuntime JsRuntime { get; set; }

        /// <summary>
        /// Specifies that barcode scanning should occur automatically whenever new image data is available.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        [Parameter] public bool AutoScan { get; set; }

        /// <summary>
        /// Specify one or more barcode types to scan for. Multiple types can be combined as flags.
        /// Setting only the barcode types applicable to your workflow can improve performance.
        /// </summary>
        /// <remarks>
        /// Defaults to all supported barcode types.
        /// </remarks>
        [Parameter] public BarcodeType ScanFor { get; set; }

        /// <summary>
        /// Barcodes with values smaller in length than the specified number of characters will not be reported.
        /// Setting to 0 will disable the minimum value check.
        /// Only applicable to barcode types that are variable in length.
        /// </summary>
        /// <remarks>
        /// Defaults to 0.
        /// </remarks>
        [Parameter] public int MinimumValueLength { get; set; }

        /// <summary>
        /// Barcodes with values greater in length than the specified number of characters will not be reported.
        /// Setting to 0 will disable the maximum value check.
        /// Only applicable to barcode types that are variable in length.
        /// </summary>
        /// <remarks>
        /// Defaults to 0.
        /// </remarks>
        [Parameter] public int MaximumValueLength { get; set; }

        /// <summary>
        /// Enable to support the full set of ASCII characters.
        /// Only applicable to barcode types that can contain non-numeric characters.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        [Parameter] public bool EnableFullCharacterSet { get; set; }

        /// <summary>
        /// Enable to require the check digit to sucessfully validate the scanned data for it to be reported.
        /// Only applicable to barcode types that contain check digits.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        [Parameter] public bool HonorCheckDigit { get; set; }

        /// <summary>
        /// Enable to include the check digit as part of the reported barcode value.
        /// Only applicable to barcode types that contain check digits.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        [Parameter] public bool IncludeCheckDigit { get; set; }

        /// <summary>
        /// Binds a callback function that will be executed when one or more barcodes were successfully identified from the provided image source.
        /// </summary>
        [Parameter] public EventCallback<ScanResult> OnBarcodesFound { get; set; }

        /// <summary>
        /// Binds a callback function that will be executed when a scan completes that did not identify any barcodes in the provided image source.
        /// </summary>
        [Parameter] public EventCallback OnBarcodesNotFound { get; set; }

        /// <summary>
        /// When enabled, additional information will be printed to the browser console at key lifecycle events. Useful for debugging purposes.
        /// </summary>
        /// <remarks>
        /// Defaults to false.
        /// </remarks>
        [Parameter] public bool Verbose { get; set; } = false;

        protected abstract ElementReference ScannerKey { get; }

        private protected readonly ScannerOptions ScannerOptions;

        internal ScannerInterop ScannerInterop;

        private protected ZBarComponent()
        {
            ScannerOptions = new();
            AutoScan = true;
            ScanFor = ScannerOptions.ScanFor;
            MinimumValueLength = ScannerOptions.MinimumValueLength;
            MaximumValueLength = ScannerOptions.MaximumValueLength;
            EnableFullCharacterSet = ScannerOptions.EnableFullCharacterSet;
            HonorCheckDigit = ScannerOptions.HonorCheckDigit;
            IncludeCheckDigit = ScannerOptions.IncludeCheckDigit;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            ScannerInterop = new ScannerInterop(JsRuntime, this); // TODO: [.NET 10] Switch to constructor injection
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            var updatedSymbolOptions = new List<SymbolOption>();

            if (parameters.TryGetValue<BarcodeType>(nameof(ScanFor), out var scanFor) && scanFor != ScanFor)
            {
                updatedSymbolOptions.AddRange(ScannerOptions.UpdateScanFor(scanFor));
            }

            if (parameters.TryGetValue<int>(nameof(MinimumValueLength), out var minimumValueLength) && minimumValueLength != MinimumValueLength)
            {
                updatedSymbolOptions.AddRange(ScannerOptions.UpdateMinValueLength(minimumValueLength));
            }

            if (parameters.TryGetValue<int>(nameof(MaximumValueLength), out var maximumValueLength) && maximumValueLength != MaximumValueLength)
            {
                updatedSymbolOptions.AddRange(ScannerOptions.UpdateMaxValueLength(maximumValueLength));
            }

            if (parameters.TryGetValue<bool>(nameof(HonorCheckDigit), out var honorCheckDigit) && honorCheckDigit != HonorCheckDigit)
            {
                updatedSymbolOptions.AddRange(ScannerOptions.UpdateHonorCheckDigit(honorCheckDigit));
            }

            if (parameters.TryGetValue<bool>(nameof(IncludeCheckDigit), out var includeCheckDigit) && includeCheckDigit != IncludeCheckDigit)
            {
                updatedSymbolOptions.AddRange(ScannerOptions.UpdateIncludeCheckDigit(includeCheckDigit));
            }

            if (parameters.TryGetValue<bool>(nameof(EnableFullCharacterSet), out var enableFullCharacterSet) && enableFullCharacterSet != EnableFullCharacterSet)
            {
                updatedSymbolOptions.AddRange(ScannerOptions.UpdateEnableFullCharacterSet(enableFullCharacterSet));
            }

            await base.SetParametersAsync(parameters);

            if (updatedSymbolOptions.Count != 0)
                await ScannerInterop.UpdateScannerConfig(ScannerKey, [.. updatedSymbolOptions], Verbose);
        }

        /// <summary>
        /// Sets the MinimumValueLength option for a specific barcode type.
        /// Multiple types can be combined as flags.
        /// </summary>
        /// <returns>
        /// Whether or not the operation was successful.
        /// The operation will only be successful for barcode types that support the MinimumValueLength option.
        /// </returns>
        /// <remarks>
        /// Use the MinimumValueLength property to configure the default for all supported barcode types.
        /// </remarks>
        internal bool OverrideMinimumValueLength(BarcodeType barcodeType, int value)
        {
            return ScannerOptions.OverrideMinimumValueLength(barcodeType, value);
        }

        /// <summary>
        /// Sets the MaximumValueLength option for a specific barcode type.
        /// Multiple types can be combined as flags.
        /// </summary>
        /// <returns>
        /// Whether or not the operation was successful.
        /// The operation will only be successful for barcode types that support the MaximumValueLength option.
        /// </returns>
        /// <remarks>
        /// Use the MaximumValueLength property to configure the default for all supported barcode types.
        /// </remarks>
        internal bool OverrideMaximumValueLength(BarcodeType barcodeType, int value)
        {
            return ScannerOptions.OverrideMaximumValueLength(barcodeType, value);
        }

        /// <summary>
        /// Sets the EnableFullCharacterSet option for a specific barcode type.
        /// Multiple types can be combined as flags.
        /// </summary>
        /// <returns>
        /// Whether or not the operation was successful.
        /// The operation will only be successful for barcode types that support the EnableFullCharacterSet option.
        /// </returns>
        /// <remarks>
        /// Use the EnableFullCharacterSet property to configure the default for all supported barcode types.
        /// </remarks>
        internal bool OverrideFullCharacterSet(BarcodeType barcodeType, bool value)
        {
            return ScannerOptions.OverrideFullCharacterSet(barcodeType, value);
        }

        /// <summary>
        /// Sets the HonorCheckDigit option for a specific barcode type.
        /// Multiple types can be combined as flags.
        /// </summary>
        /// <returns>
        /// Whether or not the operation was successful.
        /// The operation will only be successful for barcode types that support the HonorCheckDigit option.
        /// </returns>
        /// <remarks>
        /// Use the HonorCheckDigit property to configure the default for all supported barcode types.
        /// </remarks>
        internal bool OverrideHonorCheckDigit(BarcodeType barcodeType, bool value)
        {
            return ScannerOptions.OverrideHonorCheckDigit(barcodeType, value);
        }

        /// <summary>
        /// Sets the IncludeCheckDigit option for a specific barcode type.
        /// Multiple types can be combined as flags.
        /// </summary>
        /// <returns>
        /// Whether or not the operation was successful.
        /// The operation will only be successful for barcode types that support the IncludeCheckDigit option.
        /// </returns>
        /// <remarks>
        /// Use the IncludeCheckDigit property to configure the default for all supported barcode types.
        /// </remarks>
        internal bool OverrideIncludeCheckDigit(BarcodeType barcodeType, bool value)
        {
            return ScannerOptions.OverrideIncludeCheckDigit(barcodeType, value);
        }

        public virtual void Dispose()
        {
            ScannerInterop?.Dispose();
        }

        public virtual async ValueTask DisposeAsync()
        {
            await ScannerInterop.DisposeAsync();
        }
    }
}