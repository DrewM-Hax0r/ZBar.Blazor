using Microsoft.AspNetCore.Components;
using ZBar.Blazor.Config;

namespace ZBar.Blazor.Components
{
    public abstract class ZBarComponent : ComponentBase
    {
        /// <summary>
        /// Specify one or more barcode types to scan for. Multiple types can be combined as flags.
        /// Setting only the barcode types applicable to your workflow can improve performance.
        /// </summary>
        /// <remarks>
        /// Defaults to all supported barcode types.
        /// </remarks>
        [Parameter] public BarcodeType ScanFor { get; set; }

        /// <summary>
        /// Only applicable to barcode types that are variable in length.
        /// Barcodes with values smaller in length than the specified number of characters will not be reported.
        /// Setting to 0 will disable the minimum value check.
        /// </summary>
        /// <remarks>
        /// Defaults to 0.
        /// </remarks>
        [Parameter] public int MinimumValueLength { get; set; }

        /// <summary>
        /// Only applicable to barcode types that are variable in length.
        /// Barcodes with values greater in length than the specified number of characters will not be reported.
        /// Setting to 0 will disable the maximum value check.
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
        /// Only applicable to barcode types that can contain non-numeric characters.
        /// Enable to support the full set of ASCII characters.
        /// </summary>
        /// <remarks>
        /// Defaults to true.
        /// </remarks>
        [Parameter] public bool EnableFullCharacterSet { get; set; }

        private protected readonly ScannerOptions ScannerOptions;

        private protected ZBarComponent()
        {
            ScannerOptions = new();
            ScanFor = ScannerOptions.ScanFor;
            MinimumValueLength = ScannerOptions.MinimumValueLength;
            MaximumValueLength = ScannerOptions.MaximumValueLength;
            Uncertainty = ScannerOptions.Uncertainty;
            EnableFullCharacterSet = ScannerOptions.EnableFullCharacterSet;
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
    }
}