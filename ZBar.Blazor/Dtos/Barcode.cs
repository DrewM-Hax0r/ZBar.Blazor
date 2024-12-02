using ZBar.Blazor.Config;

namespace ZBar.Blazor.Dtos
{
    /// <summary>
    /// Represents a barcode that was identified from a provided image source.
    /// </summary>
    public record class Barcode
    {
        public BarcodeType Type { get; init; }
        public string Value { get; init; }
    }
}