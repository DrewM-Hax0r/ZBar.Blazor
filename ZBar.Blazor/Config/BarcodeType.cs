namespace ZBar.Blazor.Config
{
    [Flags]
    public enum BarcodeType
    {
        ALL = EAN_8 | EAN_13 | ISBN_10 | ISBN_13 | UPC_E | UPC_A | CODE_39 | CODE_128 | I25 | PDF_417 | QR_CODE,
        EAN_8 = 1,
        EAN_13 = 2,
        ISBN_10 = 4,
        ISBN_13 = 8,
        UPC_E = 16,
        UPC_A = 32,
        CODE_39 = 64,
        CODE_128 = 128,
        I25 = 256,
        PDF_417 = 512,
        QR_CODE = 1024
    }

    internal static class BarcodeTypeExtensions
    {

    }
}