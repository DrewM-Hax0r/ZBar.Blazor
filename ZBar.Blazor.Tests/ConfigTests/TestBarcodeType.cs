using ZBar.Blazor.Config;

namespace ZBar.Blazor.Tests.ConfigTests
{
    [TestClass]
    public sealed class TestBarcodeType
    {
        [TestMethod]
        public void AllType()
        {
            var expectedTypes = BarcodeTypeExtensions.IndividualBarcodeTypes();
            var allType = BarcodeType.ALL;

            foreach (var type in expectedTypes)
            {
                Assert.IsTrue(allType.HasFlag(type), $"BarcodeType: {type} flag is not set for the {allType} enum value.");
            }
        }
    }
}