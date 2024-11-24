namespace ZBar.Blazor.Config
{
    /// <summary>
    /// Encapsulates configuration options for ZBar's ImageScanner class.
    /// </summary>
    internal class ScannerOptions
    {
        private const string CONFIG_ENABLE = "ZBAR_CFG_ENABLE";
        private const string CONFIG_MIN_LEN = "ZBAR_CFG_MIN_LEN";
        private const string CONFIG_MAX_LEN = "ZBAR_CFG_MAX_LEN";
        private const string CONFIG_FULL_ASCII = "ZBAR_CFG_ASCII";

        private readonly Dictionary<BarcodeType, int> MinimumValueLengthOverrides = new();
        private readonly Dictionary<BarcodeType, int> MaximumValueLengthOverrides = new();
        private readonly Dictionary<BarcodeType, bool> EnableFullCharacterSetOverrides = new();

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
            var options = new List<SymbolOption>();
            foreach(var barcodeType in Enum.GetValues<BarcodeType>().Except([BarcodeType.ALL]))
            {
                if (ScanFor.HasFlag(barcodeType))
                {
                    options.Add(new() {
                        SymbolType = barcodeType.GetSymbolType(),
                        ConfigOptions = [
                            new() { ConfigType = CONFIG_ENABLE, Value = 1 },
                            ConfigureMinimumValueLength(barcodeType),
                            ConfigureMaximumValueLength(barcodeType),
                            ConfigureEnableFullCharacterSet(barcodeType)
                        ]
                    });
                } else {
                    options.Add(new() { SymbolType = barcodeType.GetSymbolType(), ConfigOptions = [new() { ConfigType = CONFIG_ENABLE, Value = 0 }]});
                }
            }

            return [.. options];
        }

        private ConfigOption ConfigureMinimumValueLength(BarcodeType barcodeType)
        {
            return new()
            {
                ConfigType = CONFIG_MIN_LEN,
                Value = MinimumValueLengthOverrides.TryGetValue(barcodeType, out int value) ? value : MinimumValueLength
            };
        }

        private ConfigOption ConfigureMaximumValueLength(BarcodeType barcodeType)
        {
            return new()
            {
                ConfigType = CONFIG_MAX_LEN,
                Value = MaximumValueLengthOverrides.TryGetValue(barcodeType, out int value) ? value : MaximumValueLength
            };
        }

        private ConfigOption ConfigureEnableFullCharacterSet(BarcodeType barcodeType)
        {
            var enable = EnableFullCharacterSetOverrides.TryGetValue(barcodeType, out bool value) ? value : EnableFullCharacterSet;
            return new()
            {
                ConfigType = CONFIG_FULL_ASCII,
                Value = enable ? 1 : 0
            };
        }
    }
}