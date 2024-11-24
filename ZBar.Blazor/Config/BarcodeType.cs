namespace ZBar.Blazor.Config
{
    /// <summary>
    /// Barcode types that are supported for image scanning.
    /// </summary>
    /// <remarks>
    /// Multiple types can be specified by piping together values as flags.
    /// </remarks>
    [Flags]
    public enum BarcodeType
    {
        ALL = EAN_2 | EAN_5 | EAN_8 | EAN_13 | UPC_E | UPC_A | ISBN_10 | ISBN_13 | COMPOSITE | I25 | DATABAR | DATABAR_EXPANDED | CODABAR | PDF417 | QR_CODE | QR_CODE_SECURE | CODE_39 | CODE_93 | CODE_128,
        EAN_2 = 1,
        EAN_5 = 2,
        EAN_8 = 4,
        EAN_13 = 8,
        UPC_E = 16,
        UPC_A = 32,
        ISBN_10 = 64,
        ISBN_13 = 128,
        COMPOSITE = 256,
        I25 = 512,
        DATABAR = 1024,
        DATABAR_EXPANDED = 2048,
        CODABAR = 4096,
        PDF417 = 8192,
        QR_CODE = 16384,
        QR_CODE_SECURE = 32768,
        CODE_39 = 65536,
        CODE_93 = 131072,
        CODE_128 = 262144
    }

    internal static class BarcodeTypeExtensions
    {
        public static string GetSymbolType(this BarcodeType type)
        {
            return type switch
            {
                BarcodeType.EAN_2 => "ZBAR_EAN2",
                BarcodeType.EAN_5 => "ZBAR_EAN5",
                BarcodeType.EAN_8 => "ZBAR_EAN8",
                BarcodeType.EAN_13 => "ZBAR_EAN13",
                BarcodeType.UPC_E => "ZBAR_UPCE",
                BarcodeType.UPC_A => "ZBAR_UPCA",
                BarcodeType.ISBN_10 => "ZBAR_ISBN10",
                BarcodeType.ISBN_13 => "ZBAR_ISBN13",
                BarcodeType.COMPOSITE => "ZBAR_COMPOSITE",
                BarcodeType.I25 => "ZBAR_I25",
                BarcodeType.DATABAR => "ZBAR_DATABAR",
                BarcodeType.DATABAR_EXPANDED => "ZBAR_DATABAR_EXP",
                BarcodeType.CODABAR => "ZBAR_CODABAR",
                BarcodeType.PDF417 => "ZBAR_PDF417",
                BarcodeType.QR_CODE => "ZBAR_QRCODE",
                BarcodeType.QR_CODE_SECURE => "ZBAR_SQCODE",
                BarcodeType.CODE_39 => "ZBAR_CODE39",
                BarcodeType.CODE_93 => "ZBAR_CODE93",
                BarcodeType.CODE_128 => "ZBAR_CODE128",
                _ => throw new NotImplementedException($"The symbol type for {type} is not implemented.")
            };
        }
    }
}