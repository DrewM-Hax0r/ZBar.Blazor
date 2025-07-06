using System.Xml;

namespace ZBar.Blazor.Config
{
    /// <summary>
    /// Encapsulates configuration options for ZBar's ImageScanner class.
    /// </summary>
    internal class ScannerOptions
    {
        private const string SYMBOL_ALL = "ZBAR_NONE";
        private const string CONFIG_ENABLE = "ZBAR_CFG_ENABLE";
        private const string CONFIG_MIN_LEN = "ZBAR_CFG_MIN_LEN";
        private const string CONFIG_MAX_LEN = "ZBAR_CFG_MAX_LEN";
        private const string CONFIG_UNCERTAINTY = "ZBAR_CFG_UNCERTAINTY";
        private const string CONFIG_FULL_ASCII = "ZBAR_CFG_ASCII";
        private const string CONFIG_HONOR_CHECK = "ZBAR_CFG_ADD_CHECK";
        private const string CONFIG_INCLUDE_CHECK = "ZBAR_CFG_EMIT_CHECK";

        private readonly Dictionary<BarcodeType, int> MinimumValueLengthOverrides = new();
        private readonly Dictionary<BarcodeType, int> MaximumValueLengthOverrides = new();
        private readonly Dictionary<BarcodeType, int> UncertaintyOverrides = new();
        private readonly Dictionary<BarcodeType, bool> EnableFullCharacterSetOverrides = new();
        private readonly Dictionary<BarcodeType, bool> HonorCheckDigitOverrides = new();
        private readonly Dictionary<BarcodeType, bool> IncludeCheckDigitOverrides = new();

        /// <summary>
        /// List of barcode types that support the ZBAR_CFG_MIN_LEN and ZBAR_CFG_MAX_LEN configuration settings.
        /// </summary>
        private readonly HashSet<BarcodeType> BarcodeTypesSupportingMinMaxLength = [
            BarcodeType.I25,
            BarcodeType.CODABAR,
            BarcodeType.CODE_39,
            BarcodeType.CODE_93,
            BarcodeType.CODE_128
        ];

        /// <summary>
        /// List of barcode types that support the ZBAR_CFG_ASCII configuration setting.
        /// </summary>
        private readonly HashSet<BarcodeType> BarcodeTypesSupportingFullCharacterSet = [
            BarcodeType.EAN_2,
            BarcodeType.EAN_5,
            BarcodeType.EAN_8,
            BarcodeType.EAN_13,
            BarcodeType.UPC_E,
            BarcodeType.UPC_A,
            BarcodeType.ISBN_10,
            BarcodeType.ISBN_13,
            BarcodeType.I25,
            BarcodeType.DATABAR,
            BarcodeType.DATABAR_EXPANDED,
            BarcodeType.CODABAR,
            BarcodeType.QR_CODE,
            BarcodeType.QR_CODE_SECURE,
            BarcodeType.CODE_39,
            BarcodeType.CODE_93,
            BarcodeType.CODE_128
        ];

        /// <summary>
        /// List of barcode types that support the ZBAR_CFG_ADD_CHECK and ZBAR_CFG_EMIT_CHECK configuration settings.
        /// </summary>
        private readonly HashSet<BarcodeType> BarcodeTypesSupportingCheckDigit = [
            BarcodeType.EAN_2,
            BarcodeType.EAN_5,
            BarcodeType.EAN_8,
            BarcodeType.EAN_13,
            BarcodeType.UPC_E,
            BarcodeType.UPC_A,
            BarcodeType.ISBN_10,
            BarcodeType.ISBN_13,
            BarcodeType.I25,
            BarcodeType.DATABAR,
            BarcodeType.DATABAR_EXPANDED,
            BarcodeType.CODABAR,
            BarcodeType.QR_CODE,
            BarcodeType.QR_CODE_SECURE,
            BarcodeType.CODE_39,
            BarcodeType.CODE_93,
            BarcodeType.CODE_128
        ];

        public BarcodeType ScanFor { get; private set; }
        public int MinimumValueLength { get; set; } = 0;
        public int MaximumValueLength { get; set; } = 0;
        public int Uncertainty { get; set; } = 0;
        public bool EnableFullCharacterSet { get; set; } = true;
        public bool HonorCheckDigit { get; set; } = true;
        public bool IncludeCheckDigit { get; set; } = true;

        public ScannerOptions(BarcodeType scanFor = BarcodeType.ALL)
        {
            ScanFor = scanFor;
        }

        /// <summary>
        /// Updates the current value of ScanFor.
        /// </summary>
        /// <returns>
        /// A list of symbol options to provide to ZBar to update it's scanner reflecting any changes made.
        /// </returns>
        public IList<SymbolOption> UpdateScanFor(BarcodeType newBarcodeType)
        {
            var removedUPCA = false;
            var addedUPCA = false;

            var options = new List<SymbolOption>();
            foreach (var barcodeType in BarcodeTypeExtensions.IndividualBarcodeTypes())
            {
                if (ScanFor.HasFlag(barcodeType) && !newBarcodeType.HasFlag(barcodeType))
                {
                    // Cannot remove EAN13 if UPCA is enabled (UPCA is a subset of EAN13)
                    var stopEAN13Removal = barcodeType == BarcodeType.EAN_13 && newBarcodeType.HasFlag(BarcodeType.UPC_A);

                    if (!stopEAN13Removal) options.Add(CreateSymbolOption(barcodeType, CONFIG_ENABLE, 0));
                    if (barcodeType == BarcodeType.UPC_A) removedUPCA = true;
                }
                else if (!ScanFor.HasFlag(barcodeType) && newBarcodeType.HasFlag(barcodeType))
                {
                    // Ensure configuration is up to date when enabling
                    options.Add(CreateSymbolOptionWithCurrentConfig(barcodeType));
                    if (barcodeType == BarcodeType.UPC_A) addedUPCA = true;
                }
            }

            // UPCA and EAN13 must stay in sync (UPCA is a subset of EAN13)
            if (addedUPCA && !ScanFor.HasFlag(BarcodeType.EAN_13) && !newBarcodeType.HasFlag(BarcodeType.EAN_13))
                options.Add(CreateSymbolOptionWithCurrentConfig(BarcodeType.EAN_13));
            if (removedUPCA && !ScanFor.HasFlag(BarcodeType.EAN_13) && !newBarcodeType.HasFlag(BarcodeType.EAN_13))
                options.Add(CreateSymbolOption(BarcodeType.EAN_13, CONFIG_ENABLE, 0));

            ScanFor = newBarcodeType;
            return options;
        }

        public bool OverrideMinimumValueLength(BarcodeType barcodeType, int value)
        {
            return ApplyOverride(barcodeType, value, BarcodeTypesSupportingMinMaxLength, MinimumValueLengthOverrides);
        }

        public bool OverrideMaximumValueLength(BarcodeType barcodeType, int value)
        {
            return ApplyOverride(barcodeType, value, BarcodeTypesSupportingMinMaxLength, MaximumValueLengthOverrides);
        }

        public bool OverrideUncertainty(BarcodeType barcodeType, int value)
        {
            return ApplyOverride(barcodeType, value, new HashSet<BarcodeType>(BarcodeTypeExtensions.IndividualBarcodeTypes()), UncertaintyOverrides);
        }

        public bool OverrideFullCharacterSet(BarcodeType barcodeType, bool value)
        {
            return ApplyOverride(barcodeType, value, BarcodeTypesSupportingFullCharacterSet, EnableFullCharacterSetOverrides);
        }

        public bool OverrideHonorCheckDigit(BarcodeType barcodeType, bool value)
        {
            return ApplyOverride(barcodeType, value, BarcodeTypesSupportingCheckDigit, HonorCheckDigitOverrides);
        }

        public bool OverrideIncludeCheckDigit(BarcodeType barcodeType, bool value)
        {
            return ApplyOverride(barcodeType, value, BarcodeTypesSupportingCheckDigit, IncludeCheckDigitOverrides);
        }

        /// <summary>
        /// Exports scanner configuration options in a format more easily consumed by ZBar WASM's ImageScanner.SetConfig() function.
        /// </summary>
        /// <remarks>
        /// See https://github.com/samsam2310/zbar.wasm/wiki/API-Reference#setconfig
        /// </remarks>
        public SymbolOption[] Export()
        {
            // Disable all symbol types by default - we'll manually enable the ones that are requested to scan for
            var options = new List<SymbolOption>
            {
                new() { SymbolType = SYMBOL_ALL, ConfigOptions = [new() { ConfigType = CONFIG_ENABLE, Value = 0 }] }
            };

            foreach (var barcodeType in BarcodeTypeExtensions.IndividualBarcodeTypes())
            {
                if (ScanFor.HasFlag(barcodeType))
                    options.Add(CreateSymbolOptionWithCurrentConfig(barcodeType));
            }

            // Special handling to enable EAN13 if UPCA is enabled.
            // UPCA is a subset of EAN13 and ZBar requires EAN13 to be enabled for UPCA scanning to function.
            if (ScanFor.HasFlag(BarcodeType.UPC_A) && !ScanFor.HasFlag(BarcodeType.EAN_13))
                options.Add(CreateSymbolOptionWithCurrentConfig(BarcodeType.EAN_13));

            return [.. options];
        }

        private ConfigOption[] ConfigureMinMaxValueLength(BarcodeType barcodeType)
        {
            if (!BarcodeTypesSupportingMinMaxLength.Contains(barcodeType)) return [];
            return [
                new() {
                    ConfigType = CONFIG_MIN_LEN,
                    Value = MinimumValueLengthOverrides.TryGetValue(barcodeType, out int minValue) ? minValue : MinimumValueLength
                },
                new() {
                    ConfigType = CONFIG_MAX_LEN,
                    Value = MaximumValueLengthOverrides.TryGetValue(barcodeType, out int maxValue) ? maxValue : MaximumValueLength
                }
            ];
        }

        private ConfigOption[] ConfigureUncertainty(BarcodeType barcodeType)
        {
            return [
                new() {
                    ConfigType = CONFIG_UNCERTAINTY,
                    Value = UncertaintyOverrides.TryGetValue(barcodeType, out int uncertainty) ? uncertainty : Uncertainty
                }
            ];
        }

        private ConfigOption[] ConfigureEnableFullCharacterSet(BarcodeType barcodeType)
        {
            if (!BarcodeTypesSupportingFullCharacterSet.Contains(barcodeType)) return [];
            var enable = EnableFullCharacterSetOverrides.TryGetValue(barcodeType, out bool value) ? value : EnableFullCharacterSet;
            return [new() {
                ConfigType = CONFIG_FULL_ASCII,
                Value = enable ? 1 : 0
            }];
        }

        private ConfigOption[] ConfigureCheckDigit(BarcodeType barcodeType)
        {
            if (!BarcodeTypesSupportingCheckDigit.Contains(barcodeType)) return [];
            var honorCheckDigit = HonorCheckDigitOverrides.TryGetValue(barcodeType, out bool honorCheck) ? honorCheck : HonorCheckDigit;
            var includeCheckDigit = IncludeCheckDigitOverrides.TryGetValue(barcodeType, out bool includeCheck) ? includeCheck : IncludeCheckDigit;
            return [
                new() {
                    ConfigType = CONFIG_HONOR_CHECK,
                    Value = honorCheckDigit ? 1 : 0
                },
                new() {
                    ConfigType = CONFIG_INCLUDE_CHECK,
                    Value = includeCheckDigit ? 1 : 0
                }
            ];
        }

        private bool ApplyOverride<TValue>(BarcodeType barcodeType, TValue value, HashSet<BarcodeType> supportedBarcodeTypes, IDictionary<BarcodeType, TValue> overrides)
        {
            var successful = true;
            foreach (var type in BarcodeTypeExtensions.IndividualBarcodeTypes())
            {
                if (barcodeType.HasFlag(type))
                {
                    if (supportedBarcodeTypes.Contains(type))
                        overrides[type] = value;
                    else successful = false;
                }
            }
            return successful;
        }

        private SymbolOption CreateSymbolOptionWithCurrentConfig(BarcodeType barcodeType)
        {
            return new()
            {
                SymbolType = barcodeType.GetSymbolType(),
                ConfigOptions = [
                    new() { ConfigType = CONFIG_ENABLE, Value = 1 },
                    .. ConfigureMinMaxValueLength(barcodeType),
                    .. ConfigureUncertainty(barcodeType),
                    .. ConfigureEnableFullCharacterSet(barcodeType),
                    .. ConfigureCheckDigit(barcodeType)
                ]
            };
        }

        private static SymbolOption CreateSymbolOption(BarcodeType barcodeType, string configType, int value)
        {
            return new()
            {
                SymbolType = barcodeType.GetSymbolType(),
                ConfigOptions = [
                    new() { ConfigType = CONFIG_ENABLE, Value = 0 }
                ]
            };
        }

        public record class SymbolOption
        {
            public string SymbolType { get; init; }
            public ConfigOption[] ConfigOptions { get; init; }
        }

        public record class ConfigOption
        {
            public string ConfigType { get; init; }
            public int Value { get; init; }
        }
    }
}