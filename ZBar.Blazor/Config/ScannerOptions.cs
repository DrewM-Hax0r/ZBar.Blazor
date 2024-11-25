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
        private const string CONFIG_FULL_ASCII = "ZBAR_CFG_ASCII";

        private readonly Dictionary<BarcodeType, int> MinimumValueLengthOverrides = new();
        private readonly Dictionary<BarcodeType, int> MaximumValueLengthOverrides = new();
        private readonly Dictionary<BarcodeType, bool> EnableFullCharacterSetOverrides = new();

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

        public BarcodeType ScanFor { get; set; } = BarcodeType.ALL;
        public int MinimumValueLength { get; set; } = 0;
        public int MaximumValueLength { get; set; } = 0;
        public bool EnableFullCharacterSet { get; set; } = true;

        public record class SymbolOption
        {
            public string SymbolType { get; init; }
            public ConfigOption[] ConfigOptions { get; set; }
        }

        public record class ConfigOption
        {
            public string ConfigType { get; init; }
            public int Value { get; init; }
        }

        /// <summary>
        /// Exports scanner configuration options in a format more easily consumed by ZBar's ImageScanner.SetConfig() function.
        /// </summary>
        public SymbolOption[] Export()
        {
            // Disable all symbol types by default - we'll manually enable the ones that are requested to scan for
            var options = new List<SymbolOption>
            {
                new() { SymbolType = SYMBOL_ALL, ConfigOptions = [new() { ConfigType = CONFIG_ENABLE, Value = 0 }] }
            };

            foreach (var barcodeType in Enum.GetValues<BarcodeType>().Except([BarcodeType.ALL]))
            {
                if (ScanFor.HasFlag(barcodeType))
                {
                    options.Add(new() {
                        SymbolType = barcodeType.GetSymbolType(),
                        ConfigOptions = [
                            new() { ConfigType = CONFIG_ENABLE, Value = 1 },
                            .. ConfigureMinMaxValueLength(barcodeType),
                            .. ConfigureEnableFullCharacterSet(barcodeType)
                        ]
                    });
                }
            }

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

        private ConfigOption[] ConfigureEnableFullCharacterSet(BarcodeType barcodeType)
        {
            if (!BarcodeTypesSupportingFullCharacterSet.Contains(barcodeType)) return [];
            var enable = EnableFullCharacterSetOverrides.TryGetValue(barcodeType, out bool value) ? value : EnableFullCharacterSet;
            return [new() {
                ConfigType = CONFIG_FULL_ASCII,
                Value = enable ? 1 : 0
            }];
        }
    }
}