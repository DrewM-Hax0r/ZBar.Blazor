﻿using System.Collections.ObjectModel;

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
        ALL = EAN_2 | EAN_5 | EAN_8 | EAN_13 | UPC_E | UPC_A | ISBN_10 | ISBN_13 | I25 | DATABAR | DATABAR_EXPANDED | QR_CODE | CODE_39 | CODE_93 | CODE_128,
        EAN_2 = 1,
        EAN_5 = 2,
        EAN_8 = 4,
        EAN_13 = 8,
        UPC_E = 16,
        UPC_A = 32,
        ISBN_10 = 64,
        ISBN_13 = 128,
        I25 = 256,
        DATABAR = 512,
        DATABAR_EXPANDED = 1024,
        QR_CODE = 2048,
        CODE_39 = 4096,
        CODE_93 = 8192,
        CODE_128 = 16384
    }

    internal static class BarcodeTypeExtensions
    {
        private const string EAN_2 = "ZBAR_EAN2";
        private const string EAN_5 = "ZBAR_EAN5";
        private const string EAN_8 = "ZBAR_EAN8";
        private const string EAN_13 = "ZBAR_EAN13";
        private const string UPC_E = "ZBAR_UPCE";
        private const string UPC_A = "ZBAR_UPCA";
        private const string ISBN_10 = "ZBAR_ISBN10";
        private const string ISBN_13 = "ZBAR_ISBN13";
        private const string I25 = "ZBAR_I25";
        private const string DATABAR = "ZBAR_DATABAR";
        private const string DATABAR_EXPANDED = "ZBAR_DATABAR_EXP";
        private const string QR_CODE = "ZBAR_QRCODE";
        private const string CODE_39 = "ZBAR_CODE39";
        private const string CODE_93 = "ZBAR_CODE93";
        private const string CODE_128 = "ZBAR_CODE128";

        public static BarcodeType[] IndividualBarcodeTypes()
        {
            return Enum.GetValues<BarcodeType>().Except([BarcodeType.ALL]).ToArray();
        }

        public static BarcodeType ToBarcodeType(this string type)
        {
            return type switch
            {
                EAN_2 => BarcodeType.EAN_2,
                EAN_5 => BarcodeType.EAN_5,
                EAN_8 => BarcodeType.EAN_8,
                EAN_13 => BarcodeType.EAN_13,
                UPC_E => BarcodeType.UPC_E,
                UPC_A => BarcodeType.UPC_A,
                ISBN_10 => BarcodeType.ISBN_10,
                ISBN_13 => BarcodeType.ISBN_13,
                I25 => BarcodeType.I25,
                DATABAR => BarcodeType.DATABAR,
                DATABAR_EXPANDED => BarcodeType.DATABAR_EXPANDED,
                QR_CODE => BarcodeType.QR_CODE,
                CODE_39 => BarcodeType.CODE_39,
                CODE_93 => BarcodeType.CODE_93,
                CODE_128 => BarcodeType.CODE_128,
                _ => throw new NotImplementedException($"The barcode type for {type} is not implemented.")
            };
        }

        public static string GetSymbolType(this BarcodeType type)
        {
            return type switch
            {
                BarcodeType.EAN_2 => EAN_2,
                BarcodeType.EAN_5 => EAN_5,
                BarcodeType.EAN_8 => EAN_8,
                BarcodeType.EAN_13 => EAN_13,
                BarcodeType.UPC_E => UPC_E,
                BarcodeType.UPC_A => UPC_A,
                BarcodeType.ISBN_10 => ISBN_10,
                BarcodeType.ISBN_13 => ISBN_13,
                BarcodeType.I25 => I25,
                BarcodeType.DATABAR => DATABAR,
                BarcodeType.DATABAR_EXPANDED => DATABAR_EXPANDED,
                BarcodeType.QR_CODE => QR_CODE,
                BarcodeType.CODE_39 => CODE_39,
                BarcodeType.CODE_93 => CODE_93,
                BarcodeType.CODE_128 => CODE_128,
                _ => throw new NotImplementedException($"The symbol type for {type} is not implemented.")
            };
        }

        /// <summary>
        /// ZBar requires that certain barcode types are enabled for other barcode types to be recognized
        /// </summary>
        public static readonly IReadOnlyDictionary<BarcodeType, BarcodeType[]> BarcodeDependents = new ReadOnlyDictionary<BarcodeType, BarcodeType[]>(
            new Dictionary<BarcodeType, BarcodeType[]>()
            {
                {
                    BarcodeType.EAN_13, [
                        BarcodeType.UPC_A,
                        BarcodeType.ISBN_10,
                        BarcodeType.ISBN_13
                    ]
                }
            }
        );

        public static readonly ILookup<BarcodeType, BarcodeType> BarcodeDependencies = BarcodeDependents
            .SelectMany(x => x.Value.Select(barcodeType => (x.Key, barcodeType)))
            .ToLookup(x => x.barcodeType, x => x.Key);
    }
}