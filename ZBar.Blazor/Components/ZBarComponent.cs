using Microsoft.AspNetCore.Components;
using ZBar.Blazor.Config;
using ZBar.Blazor.Dtos;
using ZBar.Blazor.Interop;

namespace ZBar.Blazor.Components
{
    public abstract class ZBarComponent : ComponentBase, IDisposable
    {
        internal readonly ScannerInterop Scanner;

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
        /// Specifies the number of sequential scans that must identify a barcode of a particular type before it is reported.
        /// Setting to 0 will disable the uncertainty.
        /// </summary>
        /// <remarks>
        /// Defaults to 0.
        /// </remarks>
        [Parameter] public int Uncertainty { get; set; }

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

        private protected readonly ScannerOptions ScannerOptions;

        private protected ZBarComponent()
        {
            Scanner = new ScannerInterop(this);
            ScannerOptions = new();
            ScanFor = ScannerOptions.ScanFor;
            MinimumValueLength = ScannerOptions.MinimumValueLength;
            MaximumValueLength = ScannerOptions.MaximumValueLength;
            Uncertainty = ScannerOptions.Uncertainty;
            EnableFullCharacterSet = ScannerOptions.EnableFullCharacterSet;
            HonorCheckDigit = ScannerOptions.HonorCheckDigit;
            IncludeCheckDigit = ScannerOptions.IncludeCheckDigit;
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);

            if (parameters.TryGetValue<BarcodeType>(nameof(ScanFor), out var scanFor))
            {
                ScannerOptions.ScanFor = scanFor;
            }

            if (parameters.TryGetValue<int>(nameof(MinimumValueLength), out var minimumValueLength))
            {
                ScannerOptions.MinimumValueLength = minimumValueLength;
            }

            if (parameters.TryGetValue<int>(nameof(MaximumValueLength), out var maximumValueLength))
            {
                ScannerOptions.MaximumValueLength = maximumValueLength;
            }

            if (parameters.TryGetValue<int>(nameof(Uncertainty), out var uncertainty))
            {
                ScannerOptions.Uncertainty = uncertainty;
            }

            if (parameters.TryGetValue<bool>(nameof(EnableFullCharacterSet), out var enableFullCharacterSet))
            {
                ScannerOptions.EnableFullCharacterSet = enableFullCharacterSet;
            }

            if (parameters.TryGetValue<bool>(nameof(HonorCheckDigit), out var honorCheckDigit))
            {
                ScannerOptions.HonorCheckDigit = honorCheckDigit;
            }

            if (parameters.TryGetValue<bool>(nameof(IncludeCheckDigit), out var includeCheckDigit))
            {
                ScannerOptions.IncludeCheckDigit = includeCheckDigit;
            }
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
        public bool OverrideMinimumValueLength(BarcodeType barcodeType, int value)
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
        public bool OverrideMaximumValueLength(BarcodeType barcodeType, int value)
        {
            return ScannerOptions.OverrideMaximumValueLength(barcodeType, value);
        }

        /// <summary>
        /// Sets the Uncertainty option for a specific barcode type.
        /// Multiple types can be combined as flags.
        /// </summary>
        /// <remarks>
        /// Use the Uncertainty property to configure the default for all barcode types.
        /// </remarks>
        public void OverrideUncertainty(BarcodeType barcodeType, int value)
        {
            ScannerOptions.OverrideUncertainty(barcodeType, value);
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
        public bool OverrideFullCharacterSet(BarcodeType barcodeType, bool value)
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
        public bool OverrideHonorCheckDigit(BarcodeType barcodeType, bool value)
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
        public bool OverrideIncludeCheckDigit(BarcodeType barcodeType, bool value)
        {
            return ScannerOptions.OverrideIncludeCheckDigit(barcodeType, value);
        }

        public void Dispose()
        {
            Scanner?.Dispose();
        }
    }
}