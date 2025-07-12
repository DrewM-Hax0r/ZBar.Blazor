using static ZBar.Blazor.Config.BarcodeTypeExtensions;

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
            BarcodeType.QR_CODE,
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
            BarcodeType.QR_CODE,
            BarcodeType.CODE_39,
            BarcodeType.CODE_93,
            BarcodeType.CODE_128
        ];

        private BarcodeType EnabledBarcodeTypes;

        public BarcodeType ScanFor { get; private set; }
        public int MinimumValueLength { get; private set; }
        public int MaximumValueLength { get; private set; }
        public int Uncertainty { get; set; } = 0;
        public bool EnableFullCharacterSet { get; set; } = true;
        public bool HonorCheckDigit { get; set; } = true;
        public bool IncludeCheckDigit { get; set; } = true;

        private int ValidateMinMaxValueLength(int value) => value < 0 ? 0 : value;

        public ScannerOptions(BarcodeType scanFor = BarcodeType.ALL, int minValueLength = 0, int maxValueLength = 0)
        {
            ScanFor = scanFor;
            InitEnabledBarcodeTypes();

            MinimumValueLength = ValidateMinMaxValueLength(minValueLength);
            MaximumValueLength = ValidateMinMaxValueLength(maxValueLength);
        }

        /// <summary>
        /// Updates the current value of ScanFor.
        /// </summary>
        /// <returns>
        /// A list of symbol options to provide to ZBar to update it's scanner reflecting any changes made.
        /// </returns>
        public IList<SymbolOption> UpdateScanFor(BarcodeType updatedScanFor)
        {
            ScanFor = updatedScanFor;
            if (updatedScanFor == BarcodeType.ALL) return Export();

            var scanForUpdates = new List<SymbolOption>();
            var scanForBarcodeTypes = new HashSet<BarcodeType>(IndividualBarcodeTypes().Where(barcodeType => updatedScanFor.HasFlag(barcodeType)));
            foreach (var barcodeType in IndividualBarcodeTypes())
            {
                if (updatedScanFor.HasFlag(barcodeType) && !EnabledBarcodeTypes.HasFlag(barcodeType))
                {
                    scanForUpdates.AddRange(EnableBarcode(barcodeType));
                    continue;
                }

                scanForUpdates.AddRange(DisableBarcodeIfRequired(barcodeType, scanForBarcodeTypes));
            }

            return scanForUpdates;
        }

        /// <summary>
        /// Updates the current value of MinimumValueLength.
        /// </summary>
        /// <returns>
        /// A list of symbol options to provide to ZBar to update it's scanner reflecting any changes made.
        /// </returns>
        public IList<SymbolOption> UpdateMinValueLength(int value)
        {
            MinimumValueLength = ValidateMinMaxValueLength(value);
            return GetMinMaxValueLengthUpdates(GetMinValueLengthConfig);
        }

        /// <summary>
        /// Updates the current value of MaximumValueLength.
        /// </summary>
        /// <returns>
        /// A list of symbol options to provide to ZBar to update it's scanner reflecting any changes made.
        /// </returns>
        public IList<SymbolOption> UpdateMaxValueLength(int value)
        {
            MaximumValueLength = ValidateMinMaxValueLength(value);
            return GetMinMaxValueLengthUpdates(GetMaxValueLengthConfig);
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
            return ApplyOverride(barcodeType, value, [.. IndividualBarcodeTypes()], UncertaintyOverrides);
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

            foreach (var barcodeType in IndividualBarcodeTypes())
            {
                if (ScanFor.HasFlag(barcodeType))
                    options.Add(CreateSymbolOptionWithCurrentConfig(barcodeType));
            }

            var dependencyOptions = InitEnabledBarcodeTypes();

            return [.. options, .. dependencyOptions];
        }

        private IList<SymbolOption> InitEnabledBarcodeTypes()
        {
            // Special handling to enable dependencies if dependents are enabled
            var results = new List<SymbolOption>();
            var barcodeTypes = ScanFor;

            foreach (var dependency in BarcodeDependents)
            {
                foreach (var dependent in dependency.Value)
                {
                    // If we added a dependent, add the dependency if needed
                    if (ScanFor.HasFlag(dependent) && !ScanFor.HasFlag(dependency.Key))
                    {
                        barcodeTypes |= dependency.Key;
                        results.Add(CreateSymbolOptionWithCurrentConfig(dependency.Key));
                        break;
                    }
                }
            }

            EnabledBarcodeTypes = barcodeTypes;
            return results;
        }

        private IList<SymbolOption> EnableBarcode(BarcodeType barcodeType)
        {
            var scanForUpdates = new List<SymbolOption>()
            {
                CreateSymbolOptionWithCurrentConfig(barcodeType)
            };
            EnabledBarcodeTypes |= barcodeType;

            foreach (var dependency in BarcodeDependencies[barcodeType])
                scanForUpdates.AddRange(EnableDependencyIfRequired(dependency));

            return scanForUpdates;
        }

        private IList<SymbolOption> EnableDependencyIfRequired(BarcodeType dependency)
        {
            return EnabledBarcodeTypes.HasFlag(dependency) ? [] : EnableBarcode(dependency);
        }

        private IList<SymbolOption> DisableBarcodeIfRequired(BarcodeType barcodeType, HashSet<BarcodeType> scanForBarcodeTypes)
        {
            var scanForUpdates = new List<SymbolOption>();
            if (EnabledBarcodeTypes.HasFlag(barcodeType) && !scanForBarcodeTypes.Contains(barcodeType))
            {
                if (!scanForBarcodeTypes.SelectMany(type => BarcodeDependencies[type]).Contains(barcodeType))
                {
                    scanForUpdates.Add(CreateSymbolOption(barcodeType, CONFIG_ENABLE, 0));
                    EnabledBarcodeTypes &= ~barcodeType;

                    if (BarcodeDependents.ContainsKey(barcodeType))
                    {
                        foreach (var dependant in BarcodeDependents[barcodeType])
                        {
                            scanForUpdates.AddRange(DisableBarcodeIfRequired(dependant, scanForBarcodeTypes));
                        }
                    }
                }
            }
            return scanForUpdates;
        }

        private IList<SymbolOption> GetMinMaxValueLengthUpdates(Func<BarcodeType, ConfigOption> getConfig)
        {
            var updates = new List<SymbolOption>();
            foreach (var barcodeType in BarcodeTypesSupportingMinMaxLength)
            {
                if (EnabledBarcodeTypes.HasFlag(barcodeType))
                    updates.Add(CreateSymbolOption(barcodeType, getConfig));
            }

            return updates;
        }

        private ConfigOption GetMinValueLengthConfig(BarcodeType barcodeType)
        {
            return new() {
                ConfigType = CONFIG_MIN_LEN,
                Value = MinimumValueLengthOverrides.TryGetValue(barcodeType, out int minValue) ? minValue : MinimumValueLength
            };
        }

        private ConfigOption GetMaxValueLengthConfig(BarcodeType barcodeType)
        {
            return new() {
                ConfigType = CONFIG_MAX_LEN,
                Value = MaximumValueLengthOverrides.TryGetValue(barcodeType, out int maxValue) ? maxValue : MaximumValueLength
            };
        }

        private ConfigOption[] ConfigureMinMaxValueLength(BarcodeType barcodeType)
        {
            if (!BarcodeTypesSupportingMinMaxLength.Contains(barcodeType)) return [];
            return [
                GetMinValueLengthConfig(barcodeType),
                GetMaxValueLengthConfig(barcodeType)
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
            foreach (var type in IndividualBarcodeTypes())
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

        private static SymbolOption CreateSymbolOption(BarcodeType barcodeType, params Func<BarcodeType, ConfigOption>[] configOptions)
        {
            return new()
            {
                SymbolType = barcodeType.GetSymbolType(),
                ConfigOptions = [.. configOptions.Select(cfg => cfg(barcodeType))]
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