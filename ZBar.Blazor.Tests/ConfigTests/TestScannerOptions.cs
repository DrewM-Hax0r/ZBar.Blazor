using ZBar.Blazor.Config;

namespace ZBar.Blazor.Tests.ConfigTests
{
    [TestClass]
    public class TestScannerOptions
    {
        private const string SYMBOL_ALL = "ZBAR_NONE";
        private const string CONFIG_ENABLE = "ZBAR_CFG_ENABLE";
        private const string CONFIG_MIN_LEN = "ZBAR_CFG_MIN_LEN";
        private const string CONFIG_MAX_LEN = "ZBAR_CFG_MAX_LEN";
        private const string CONFIG_UNCERTAINTY = "ZBAR_CFG_UNCERTAINTY";
        private const string CONFIG_FULL_ASCII = "ZBAR_CFG_ASCII";

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

        [TestMethod]
        public void Defaults()
        {
            var options = new ScannerOptions();

            Assert.AreEqual(BarcodeType.ALL, options.ScanFor);
            Assert.AreEqual(0, options.MaximumValueLength);
            Assert.AreEqual(0, options.MaximumValueLength);
            Assert.IsTrue(options.EnableFullCharacterSet);
        }

        [TestMethod]
        public void Export_All()
        {
            var options = new ScannerOptions() { ScanFor = BarcodeType.ALL };
            var export = options.Export();

            Assert.AreEqual(Enum.GetValues<BarcodeType>().Length, export.Length);

            AssertSymbol(export[0], null, 1);
            AssertConfig(export[0].ConfigOptions[0], CONFIG_ENABLE, 0);

            foreach (var barcodeType in BarcodeTypeExtensions.IndividualBarcodeTypes())
            {
                var symbol = AssertSymbolConfigured(export, barcodeType);
                AssertConfig(symbol.ConfigOptions[0], CONFIG_ENABLE, 1);
            }
        }

        [TestMethod]
        public void Export_Specific()
        {
            var options = new ScannerOptions() { ScanFor = BarcodeType.EAN_13 | BarcodeType.QR_CODE };
            var export = options.Export();

            Assert.AreEqual(3, export.Length);

            AssertSymbol(export[0], null, 1);
            AssertConfig(export[0].ConfigOptions[0], CONFIG_ENABLE, 0);

            var symbol = AssertSymbolConfigured(export, BarcodeType.EAN_13);
            AssertConfig(symbol.ConfigOptions[0], CONFIG_ENABLE, 1);

            symbol = AssertSymbolConfigured(export, BarcodeType.QR_CODE);
            AssertConfig(symbol.ConfigOptions[0], CONFIG_ENABLE, 1);
        }

        [TestMethod]
        public void Export_MinMaxLength()
        {
            foreach (var barcodeType in BarcodeTypesSupportingMinMaxLength)
            {
                var options = new ScannerOptions()
                {
                    ScanFor = barcodeType,
                    MinimumValueLength = 50,
                    MaximumValueLength = 200
                };

                var export = options.Export();

                var symbol = AssertSymbolConfigured(export, barcodeType);
                AssertConfigValue(symbol, CONFIG_MIN_LEN, 50);
                AssertConfigValue(symbol, CONFIG_MAX_LEN, 200);
            }
        }

        [TestMethod]
        public void Export_OverrideMinimumValueLength()
        {
            var options = new ScannerOptions()
            {
                ScanFor = BarcodeType.ALL,
                MinimumValueLength = 50
            };
            options.OverrideMinimumValueLength(BarcodeType.I25 | BarcodeType.CODE_39, 100);

            var export = options.Export();

            foreach (var barcodeType in BarcodeTypesSupportingMinMaxLength)
            {
                var symbol = AssertSymbolConfigured(export, barcodeType);
                var overridden = barcodeType == BarcodeType.I25 || barcodeType == BarcodeType.CODE_39;
                AssertConfigValue(symbol, CONFIG_MIN_LEN, overridden ? 100 : 50);
            }
        }

        [TestMethod]
        public void Export_OverrideMaximumValueLength()
        {
            var options = new ScannerOptions()
            {
                ScanFor = BarcodeType.ALL,
                MaximumValueLength = 100
            };
            options.OverrideMaximumValueLength(BarcodeType.I25 | BarcodeType.CODE_39, 200);

            var export = options.Export();

            foreach (var barcodeType in BarcodeTypesSupportingMinMaxLength)
            {
                var symbol = AssertSymbolConfigured(export, barcodeType);
                var overridden = barcodeType == BarcodeType.I25 || barcodeType == BarcodeType.CODE_39;
                AssertConfigValue(symbol, CONFIG_MAX_LEN, overridden ? 200 : 100);
            }
        }

        [TestMethod]
        public void Export_BarcodeTypesExcludingMinMaxLength()
        {
            var unsupportedTypes = Enum.GetValues<BarcodeType>().Except([
                BarcodeType.ALL,
                .. BarcodeTypesSupportingMinMaxLength
            ]).ToArray();

            foreach (var unsupportedType in unsupportedTypes)
            {
                var options = new ScannerOptions() { ScanFor = unsupportedType };
                var export = options.Export();

                var symbol = AssertSymbolConfigured(export, unsupportedType);
                Assert.IsFalse(ContainsConfigOption(symbol, CONFIG_MAX_LEN));
                Assert.IsFalse(ContainsConfigOption(symbol, CONFIG_MIN_LEN));
            };
        }

        [TestMethod]
        public void Export_Uncertainty()
        {
            foreach (var barcodeType in BarcodeTypeExtensions.IndividualBarcodeTypes())
            {
                var options = new ScannerOptions()
                {
                    ScanFor = barcodeType,
                    Uncertainty = 2
                };

                var export = options.Export();

                var symbol = AssertSymbolConfigured(export, barcodeType);
                AssertConfigValue(symbol, CONFIG_UNCERTAINTY, 2);
            }
        }

        [TestMethod]
        public void Export_OverrideUncertainty()
        {
            var options = new ScannerOptions()
            {
                ScanFor = BarcodeType.ALL,
                Uncertainty = 0
            };
            options.OverrideUncertainty(BarcodeType.UPC_A | BarcodeType.ISBN_13, 3);

            var export = options.Export();

            foreach (var barcodeType in BarcodeTypeExtensions.IndividualBarcodeTypes())
            {
                var symbol = AssertSymbolConfigured(export, barcodeType);
                var overridden = barcodeType == BarcodeType.UPC_A || barcodeType == BarcodeType.ISBN_13;
                AssertConfigValue(symbol, CONFIG_UNCERTAINTY, overridden ? 3 : 0);
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Export_EnableFullCharacterSet(bool enable)
        {
            foreach (var barcodeType in BarcodeTypesSupportingFullCharacterSet)
            {
                var options = new ScannerOptions()
                {
                    ScanFor = barcodeType,
                    EnableFullCharacterSet = enable
                };

                var export = options.Export();

                var symbol = AssertSymbolConfigured(export, barcodeType);
                AssertConfigValue(symbol, CONFIG_FULL_ASCII, enable ? 1 : 0);
            }
        }

        [TestMethod]
        public void Export_OverrideFullCharacterSet()
        {
            var options = new ScannerOptions()
            {
                ScanFor = BarcodeType.ALL,
                EnableFullCharacterSet = false
            };
            options.OverrideFullCharacterSet(BarcodeType.EAN_5 | BarcodeType.I25, true);

            var export = options.Export();

            foreach (var barcodeType in BarcodeTypesSupportingFullCharacterSet)
            {
                var symbol = AssertSymbolConfigured(export, barcodeType);
                var overridden = barcodeType == BarcodeType.EAN_5 || barcodeType == BarcodeType.I25;
                AssertConfigValue(symbol, CONFIG_FULL_ASCII, overridden ? 1 : 0);
            }
        }

        [TestMethod]
        public void Export_BarcodeTypesExcludingFullCharacterSet()
        {
            var unsupportedTypes = Enum.GetValues<BarcodeType>().Except([
                BarcodeType.ALL,
                .. BarcodeTypesSupportingFullCharacterSet
            ]).ToArray();

            foreach (var unsupportedType in unsupportedTypes)
            {
                var options = new ScannerOptions() { ScanFor = unsupportedType };
                var export = options.Export();

                var symbol = AssertSymbolConfigured(export, unsupportedType);
                Assert.IsFalse(ContainsConfigOption(symbol, CONFIG_FULL_ASCII));
            };
        }

        private static bool ContainsConfigOption(ScannerOptions.SymbolOption symbol, string optionName)
        {
            return symbol.ConfigOptions.Any(cfg => cfg.ConfigType == optionName);
        }

        private static ScannerOptions.SymbolOption AssertSymbolConfigured(ScannerOptions.SymbolOption[] export, BarcodeType barcodeType)
        {
            var symbol = export.Single(x => x.SymbolType == barcodeType.GetSymbolType());
            Assert.IsNotNull(symbol);
            return symbol;
        }

        private static void AssertConfigValue(ScannerOptions.SymbolOption symbol, string configType, int value)
        {
            var config = symbol.ConfigOptions.Single(x => x.ConfigType == configType);
            Assert.IsNotNull(config);
            Assert.AreEqual(value, config.Value);
        }

        private static void AssertSymbol(ScannerOptions.SymbolOption symbol, BarcodeType? barcodeType, int numConfigOptions)
        {
            var symbolType = barcodeType?.GetSymbolType() ?? SYMBOL_ALL;
            Assert.AreEqual(symbolType, symbol.SymbolType);
            Assert.AreEqual(numConfigOptions, symbol.ConfigOptions.Length);
        }

        private static void AssertConfig(ScannerOptions.ConfigOption config, string configType, int value)
        {
            Assert.AreEqual(configType, config.ConfigType);
            Assert.AreEqual(value, config.Value);
        }
    }
}