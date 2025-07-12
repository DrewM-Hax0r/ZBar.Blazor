using ZBar.Blazor.Config;
using static ZBar.Blazor.Config.ScannerOptions;

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
        private const string CONFIG_HONOR_CHECK = "ZBAR_CFG_ADD_CHECK";
        private const string CONFIG_INCLUDE_CHECK = "ZBAR_CFG_EMIT_CHECK";

        /// <summary>
        /// List of barcode types that support the ZBAR_CFG_MIN_LEN and ZBAR_CFG_MAX_LEN configuration settings.
        /// </summary>
        private static readonly HashSet<BarcodeType> BarcodeTypesSupportingMinMaxLength = [
            BarcodeType.I25,
            BarcodeType.CODE_39,
            BarcodeType.CODE_93,
            BarcodeType.CODE_128
        ];

        /// <summary>
        /// List of barcode types that support the ZBAR_CFG_ASCII configuration setting.
        /// </summary>
        private static readonly HashSet<BarcodeType> BarcodeTypesSupportingFullCharacterSet = [
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
        private static readonly HashSet<BarcodeType> BarcodeTypesSupportingCheckDigit = [
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
        public void UpdateScanFor()
        {
            var options = new ScannerOptions(scanFor: BarcodeType.UPC_E | BarcodeType.ISBN_13);
            var results = options.UpdateScanFor(BarcodeType.ISBN_13 | BarcodeType.QR_CODE);

            Assert.AreEqual(BarcodeType.ISBN_13 | BarcodeType.QR_CODE, options.ScanFor);
            Assert.AreEqual(2, results.Count);

            var symbol = AssertSymbolConfigured(results, BarcodeType.UPC_E);
            AssertSymbol(symbol, BarcodeType.UPC_E, 1);
            AssertConfig(symbol.ConfigOptions[0], CONFIG_ENABLE, 0);

            symbol = AssertSymbolConfigured(results, BarcodeType.QR_CODE);
            AssertSymbolAllConfigurationsSet(symbol, BarcodeType.QR_CODE, enableFlag: 1, options);
        }

        /// <summary>
        /// ZBar requires that EAN13 is enabled if certain other barcode types are enabled
        /// </summary>
        [DataTestMethod]
        [DataRow(BarcodeType.UPC_A)]
        [DataRow(BarcodeType.ISBN_10)]
        [DataRow(BarcodeType.ISBN_13)]
        public void UpdateScanFor_Dependents_On_EAN13(BarcodeType dependentBarcodeType)
        {
            // Case 1: Adding dependent when EAN13 is not already enabled also enables EAN13
            var options = new ScannerOptions(scanFor: BarcodeType.I25);
            var results = options.UpdateScanFor(BarcodeType.I25 | dependentBarcodeType);

            Assert.AreEqual(2, results.Count);

            var symbol = AssertSymbolConfigured(results, dependentBarcodeType);
            AssertSymbolAllConfigurationsSet(symbol, dependentBarcodeType, enableFlag: 1, options);

            symbol = AssertSymbolConfigured(results, BarcodeType.EAN_13);
            AssertSymbolAllConfigurationsSet(symbol, BarcodeType.EAN_13, enableFlag: 1, options);


            // Case 2: Removing dependent when EAN13 is not enabled also removes EAN13
            options = new ScannerOptions(scanFor: BarcodeType.I25 | dependentBarcodeType);
            results = options.UpdateScanFor(BarcodeType.I25);

            Assert.AreEqual(2, results.Count);

            symbol = AssertSymbolConfigured(results, dependentBarcodeType);
            AssertSymbol(symbol, dependentBarcodeType, 1);
            AssertConfig(symbol.ConfigOptions[0], CONFIG_ENABLE, 0);

            symbol = AssertSymbolConfigured(results, BarcodeType.EAN_13);
            AssertSymbol(symbol, BarcodeType.EAN_13, 1);
            AssertConfig(symbol.ConfigOptions[0], CONFIG_ENABLE, 0);


            // Case 3: Removing dependent when EAN13 is enabled leaves EAN13 enabled
            options = new ScannerOptions(scanFor: BarcodeType.I25 | dependentBarcodeType | BarcodeType.EAN_13);
            results = options.UpdateScanFor(BarcodeType.I25 | BarcodeType.EAN_13);

            Assert.AreEqual(1, results.Count);

            symbol = AssertSymbolConfigured(results, dependentBarcodeType);
            AssertSymbol(symbol, dependentBarcodeType, 1);
            AssertConfig(symbol.ConfigOptions[0], CONFIG_ENABLE, 0);


            // Case 4: Removing EAN13 when dependent is enabled leaves EAN13 enabled
            options = new ScannerOptions(scanFor: BarcodeType.I25 | dependentBarcodeType | BarcodeType.EAN_13);
            results = options.UpdateScanFor(BarcodeType.I25 | dependentBarcodeType);

            Assert.AreEqual(0, results.Count);

            // Case 5: Adding dependent when EAN13 is enabled does not modify EAN13
            options = new ScannerOptions(scanFor: BarcodeType.I25 | BarcodeType.EAN_13);
            results = options.UpdateScanFor(BarcodeType.I25 | BarcodeType.EAN_13 | dependentBarcodeType);

            symbol = AssertSymbolConfigured(results, dependentBarcodeType);
            AssertSymbolAllConfigurationsSet(symbol, dependentBarcodeType, enableFlag: 1, options);
        }

        [DataTestMethod]
        [DataRow(5, DisplayName = "Regular Value")]
        [DataRow(-1, DisplayName = "Validation")]
        public void UpdateMinValueLength(int value)
        {
            var options = new ScannerOptions(scanFor: BarcodeType.I25 | BarcodeType.CODE_39 | BarcodeType.EAN_8 | BarcodeType.CODE_128);
            var results = options.UpdateMinValueLength(value);

            var expectedValue = value < 0 ? 0 : value;

            Assert.AreEqual(expectedValue, options.MinimumValueLength);
            Assert.AreEqual(3, results.Count);

            var symbol = AssertSymbolConfigured(results, BarcodeType.I25);
            AssertSymbol(symbol, BarcodeType.I25, 1);
            AssertConfig(symbol.ConfigOptions[0], CONFIG_MIN_LEN, expectedValue);

            symbol = AssertSymbolConfigured(results, BarcodeType.CODE_39);
            AssertSymbol(symbol, BarcodeType.CODE_39, 1);
            AssertConfig(symbol.ConfigOptions[0], CONFIG_MIN_LEN, expectedValue);

            symbol = AssertSymbolConfigured(results, BarcodeType.CODE_128);
            AssertSymbol(symbol, BarcodeType.CODE_128, 1);
            AssertConfig(symbol.ConfigOptions[0], CONFIG_MIN_LEN, expectedValue);
        }

        [DataTestMethod]
        [DataRow(5, DisplayName = "Regular Value")]
        [DataRow(-1, DisplayName = "Validation")]
        public void UpdateMaxValueLength(int value)
        {
            var options = new ScannerOptions(scanFor: BarcodeType.I25 | BarcodeType.CODE_39 | BarcodeType.EAN_8 | BarcodeType.CODE_128);
            var results = options.UpdateMaxValueLength(value);

            var expectedValue = value < 0 ? 0 : value;

            Assert.AreEqual(expectedValue, options.MaximumValueLength);
            Assert.AreEqual(3, results.Count);

            var symbol = AssertSymbolConfigured(results, BarcodeType.I25);
            AssertSymbol(symbol, BarcodeType.I25, 1);
            AssertConfig(symbol.ConfigOptions[0], CONFIG_MAX_LEN, expectedValue);

            symbol = AssertSymbolConfigured(results, BarcodeType.CODE_39);
            AssertSymbol(symbol, BarcodeType.CODE_39, 1);
            AssertConfig(symbol.ConfigOptions[0], CONFIG_MAX_LEN, expectedValue);

            symbol = AssertSymbolConfigured(results, BarcodeType.CODE_128);
            AssertSymbol(symbol, BarcodeType.CODE_128, 1);
            AssertConfig(symbol.ConfigOptions[0], CONFIG_MAX_LEN, expectedValue);
        }

        [TestMethod]
        public void Export_All()
        {
            var options = new ScannerOptions(scanFor: BarcodeType.ALL);
            var export = options.Export();

            Assert.AreEqual(Enum.GetValues<BarcodeType>().Length, export.Length);

            AssertSymbol(export[0], null, 1);
            AssertConfig(export[0].ConfigOptions[0], CONFIG_ENABLE, 0);

            foreach (var barcodeType in BarcodeTypeExtensions.IndividualBarcodeTypes())
            {
                var symbol = AssertSymbolConfigured(export, barcodeType);
                AssertSymbolAllConfigurationsSet(symbol, barcodeType, enableFlag: 1, options);
            }
        }

        [TestMethod]
        public void Export_Specific()
        {
            var options = new ScannerOptions(scanFor: BarcodeType.EAN_13 | BarcodeType.QR_CODE);
            var export = options.Export();

            Assert.AreEqual(3, export.Length);

            AssertSymbol(export[0], null, 1);
            AssertConfig(export[0].ConfigOptions[0], CONFIG_ENABLE, 0);

            var symbol = AssertSymbolConfigured(export, BarcodeType.EAN_13);
            AssertSymbolAllConfigurationsSet(symbol, BarcodeType.EAN_13, enableFlag: 1, options);

            symbol = AssertSymbolConfigured(export, BarcodeType.QR_CODE);
            AssertSymbolAllConfigurationsSet(symbol, BarcodeType.QR_CODE, enableFlag: 1, options);
        }

        /// <summary>
        /// ZBar requires that EAN13 is enabled if certain other barcode types are enabled
        /// </summary>
        [DataTestMethod]
        [DataRow(BarcodeType.UPC_A)]
        [DataRow(BarcodeType.ISBN_10)]
        [DataRow(BarcodeType.ISBN_13)]
        public void Export_Specific_Dependents_On_EAN13(BarcodeType dependentBarcodeType)
        {
            var options = new ScannerOptions(scanFor: dependentBarcodeType);
            var export = options.Export();

            Assert.AreEqual(3, export.Length);

            AssertSymbol(export[0], null, 1);
            AssertConfig(export[0].ConfigOptions[0], CONFIG_ENABLE, 0);

            var symbol = AssertSymbolConfigured(export, dependentBarcodeType);
            AssertSymbolAllConfigurationsSet(symbol, dependentBarcodeType, enableFlag: 1, options);

            symbol = AssertSymbolConfigured(export, BarcodeType.EAN_13);
            AssertSymbolAllConfigurationsSet(symbol, BarcodeType.EAN_13, enableFlag: 1, options);
        }

        [TestMethod]
        public void Export_MinMaxLength()
        {
            foreach (var barcodeType in BarcodeTypesSupportingMinMaxLength)
            {
                var options = new ScannerOptions(scanFor: barcodeType, minValueLength: 50, maxValueLength: 200);
                var export = options.Export();

                var symbol = AssertSymbolConfigured(export, barcodeType);
                AssertConfigValue(symbol, CONFIG_MIN_LEN, 50);
                AssertConfigValue(symbol, CONFIG_MAX_LEN, 200);
            }
        }

        [TestMethod]
        public void Export_OverrideMinimumValueLength()
        {
            var options = new ScannerOptions(scanFor: BarcodeType.ALL, minValueLength: 50);
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
            var options = new ScannerOptions(scanFor: BarcodeType.ALL, maxValueLength: 100);
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
                var options = new ScannerOptions(scanFor: unsupportedType);
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
                var options = new ScannerOptions(scanFor: barcodeType)
                {
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
            var options = new ScannerOptions(scanFor: BarcodeType.ALL)
            {
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
                var options = new ScannerOptions(scanFor: barcodeType)
                {
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
            var options = new ScannerOptions(scanFor: BarcodeType.ALL)
            {
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
                var options = new ScannerOptions(scanFor: unsupportedType);
                var export = options.Export();

                var symbol = AssertSymbolConfigured(export, unsupportedType);
                Assert.IsFalse(ContainsConfigOption(symbol, CONFIG_FULL_ASCII));
            };
        }

        [TestMethod]
        public void Export_HonorCheckDigit()
        {
            foreach (var barcodeType in BarcodeTypesSupportingCheckDigit)
            {
                var options = new ScannerOptions(scanFor: barcodeType)
                {
                    HonorCheckDigit = true
                };

                var export = options.Export();

                var symbol = AssertSymbolConfigured(export, barcodeType);
                AssertConfigValue(symbol, CONFIG_HONOR_CHECK, 1);
            }
        }

        [TestMethod]
        public void Export_OverrideHonorCheckDigit()
        {
            var options = new ScannerOptions(scanFor: BarcodeType.ALL)
            {
                HonorCheckDigit = true
            };
            options.OverrideHonorCheckDigit(BarcodeType.EAN_13 | BarcodeType.ISBN_13, false);

            var export = options.Export();

            foreach (var barcodeType in BarcodeTypesSupportingCheckDigit)
            {
                var symbol = AssertSymbolConfigured(export, barcodeType);
                var overridden = barcodeType == BarcodeType.EAN_13 || barcodeType == BarcodeType.ISBN_13;
                AssertConfigValue(symbol, CONFIG_HONOR_CHECK, overridden ? 0 : 1);
            }
        }

        [TestMethod]
        public void Export_IncludeCheckDigit()
        {
            foreach (var barcodeType in BarcodeTypesSupportingCheckDigit)
            {
                var options = new ScannerOptions(scanFor: barcodeType)
                {
                    IncludeCheckDigit = true
                };

                var export = options.Export();

                var symbol = AssertSymbolConfigured(export, barcodeType);
                AssertConfigValue(symbol, CONFIG_INCLUDE_CHECK, 1);
            }
        }

        [TestMethod]
        public void Export_OverrideIncludeCheckDigit()
        {
            var options = new ScannerOptions(scanFor: BarcodeType.ALL)
            {
                IncludeCheckDigit = true
            };
            options.OverrideIncludeCheckDigit(BarcodeType.EAN_13 | BarcodeType.ISBN_13, false);

            var export = options.Export();

            foreach (var barcodeType in BarcodeTypesSupportingCheckDigit)
            {
                var symbol = AssertSymbolConfigured(export, barcodeType);
                var overridden = barcodeType == BarcodeType.EAN_13 || barcodeType == BarcodeType.ISBN_13;
                AssertConfigValue(symbol, CONFIG_INCLUDE_CHECK, overridden ? 0 : 1);
            }
        }

        [TestMethod]
        public void Export_BarcodeTypesExcludingCheckDigit()
        {
            var unsupportedTypes = Enum.GetValues<BarcodeType>().Except([
                BarcodeType.ALL,
                .. BarcodeTypesSupportingCheckDigit
            ]).ToArray();

            foreach (var unsupportedType in unsupportedTypes)
            {
                var options = new ScannerOptions(scanFor: unsupportedType);
                var export = options.Export();

                var symbol = AssertSymbolConfigured(export, unsupportedType);
                Assert.IsFalse(ContainsConfigOption(symbol, CONFIG_HONOR_CHECK));
                Assert.IsFalse(ContainsConfigOption(symbol, CONFIG_INCLUDE_CHECK));
            };
        }

        private static bool ContainsConfigOption(ScannerOptions.SymbolOption symbol, string optionName)
        {
            return symbol.ConfigOptions.Any(cfg => cfg.ConfigType == optionName);
        }

        private static ScannerOptions.SymbolOption AssertSymbolConfigured(IList<ScannerOptions.SymbolOption> export, BarcodeType barcodeType)
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

        private static void AssertSymbolAllConfigurationsSet(SymbolOption actual, BarcodeType barcodeType, int enableFlag, ScannerOptions scannerOptions)
        {
            Assert.IsNotNull(actual?.ConfigOptions);

            int configLines = 0;
            AssertConfig(actual.ConfigOptions[configLines], CONFIG_ENABLE, enableFlag);
            configLines++;

            if (BarcodeTypesSupportingMinMaxLength.Contains(barcodeType))
            {
                AssertConfig(actual.ConfigOptions[configLines], CONFIG_MIN_LEN, scannerOptions.MinimumValueLength);
                configLines++;

                AssertConfig(actual.ConfigOptions[configLines], CONFIG_MAX_LEN, scannerOptions.MaximumValueLength);
                configLines++;
            }

            AssertConfig(actual.ConfigOptions[configLines], CONFIG_UNCERTAINTY, scannerOptions.Uncertainty);
            configLines++;

            if (BarcodeTypesSupportingFullCharacterSet.Contains(barcodeType))
            {
                AssertConfig(actual.ConfigOptions[configLines], CONFIG_FULL_ASCII, scannerOptions.EnableFullCharacterSet ? 1 : 0);
                configLines++;
            }

            if (BarcodeTypesSupportingCheckDigit.Contains(barcodeType))
            {
                AssertConfig(actual.ConfigOptions[configLines], CONFIG_HONOR_CHECK, scannerOptions.HonorCheckDigit ? 1 : 0);
                configLines++;

                AssertConfig(actual.ConfigOptions[configLines], CONFIG_INCLUDE_CHECK, scannerOptions.IncludeCheckDigit ? 1 : 0);
                configLines++;
            }

            AssertSymbol(actual, barcodeType, configLines);
        }
    }
}