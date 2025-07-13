using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ZBar.Blazor.Config;

namespace ZBar.Blazor.Sandbox.Components
{
    public partial class AdvancedConfiguration
    {
        [CascadingParameter] BlazoredModalInstance ModalInstance { get; set; }

        [Parameter] public BarcodeType ScanFor { get; set; }
        [Parameter] public int MinValueLength { get; set; }
        [Parameter] public int MaxValueLength { get; set; }

        private bool AllBarcodeTypes { get; set; }
        private bool CustomUPCA { get; set; }
        private bool CustomUPCE { get; set; }
        private bool CustomISBN13 { get; set; }
        private bool CustomISBN10 { get; set; }
        private bool CustomEAN13 { get; set; }
        private bool CustomEAN8 { get; set; }
        private bool CustomEAN5 { get; set; }
        private bool CustomEAN2 { get; set; }
        private bool CustomI25 { get; set; }
        private bool CustomDatabar { get; set; }
        private bool CustomDatabarExpanded { get; set; }
        private bool CustomCode39 { get; set; }
        private bool CustomCode93 { get; set; }
        private bool CustomCode128 { get; set; }
        private bool CustomQR { get; set; }

        private int MinValueLengthEdit { get; set; }
        private int MaxValueLengthEdit { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            AllBarcodeTypes = ScanFor == BarcodeType.ALL;
            if (!AllBarcodeTypes)
            {
                CustomUPCA = ScanFor.HasFlag(BarcodeType.UPC_A);
                CustomUPCE = ScanFor.HasFlag(BarcodeType.UPC_E);
                CustomISBN13 = ScanFor.HasFlag(BarcodeType.ISBN_13);
                CustomISBN10 = ScanFor.HasFlag(BarcodeType.ISBN_10);
                CustomEAN13 = ScanFor.HasFlag(BarcodeType.EAN_13);
                CustomEAN8 = ScanFor.HasFlag(BarcodeType.EAN_8);
                CustomEAN5 = ScanFor.HasFlag(BarcodeType.EAN_5);
                CustomEAN2 = ScanFor.HasFlag(BarcodeType.EAN_2);
                CustomI25 = ScanFor.HasFlag(BarcodeType.I25);
                CustomDatabar = ScanFor.HasFlag(BarcodeType.DATABAR);
                CustomDatabarExpanded = ScanFor.HasFlag(BarcodeType.DATABAR_EXPANDED);
                CustomCode39 = ScanFor.HasFlag(BarcodeType.CODE_39);
                CustomCode93 = ScanFor.HasFlag(BarcodeType.CODE_93);
                CustomCode128 = ScanFor.HasFlag(BarcodeType.CODE_128);
                CustomQR = ScanFor.HasFlag(BarcodeType.QR_CODE);
            }

            MinValueLengthEdit = MinValueLength;
            MaxValueLengthEdit = MaxValueLength;
        }

        private void EnableAllBarcodes()
        {
            CustomUPCA = false;
            CustomUPCE = false;
            CustomISBN13 = false;
            CustomISBN10 = false;
            CustomEAN13 = false;
            CustomEAN8 = false;
            CustomEAN5 = false;
            CustomEAN2 = false;
            CustomI25 = false;
            CustomDatabar = false;
            CustomDatabarExpanded = false;
            CustomCode39 = false;
            CustomCode93 = false;
            CustomCode128 = false;
            CustomQR = false;

            AllBarcodeTypes = true;
        }

        private async Task CloseOk()
        {
            var scanFor = BarcodeType.ALL;
            if (!AllBarcodeTypes) {
                scanFor = 0;
                if (CustomUPCA) scanFor = scanFor | BarcodeType.UPC_A;
                if (CustomUPCE) scanFor = scanFor | BarcodeType.UPC_E;
                if (CustomISBN13) scanFor = scanFor | BarcodeType.ISBN_13;
                if (CustomISBN10) scanFor = scanFor | BarcodeType.ISBN_10;
                if (CustomEAN13) scanFor = scanFor | BarcodeType.EAN_13;
                if (CustomEAN8) scanFor = scanFor | BarcodeType.EAN_8;
                if (CustomEAN5) scanFor = scanFor | BarcodeType.EAN_5;
                if (CustomEAN2) scanFor = scanFor | BarcodeType.EAN_2;
                if (CustomI25) scanFor = scanFor | BarcodeType.I25;
                if (CustomDatabar) scanFor = scanFor | BarcodeType.DATABAR;
                if (CustomDatabarExpanded) scanFor = scanFor | BarcodeType.DATABAR_EXPANDED;
                if (CustomCode39) scanFor = scanFor | BarcodeType.CODE_39;
                if (CustomCode93) scanFor = scanFor | BarcodeType.CODE_93;
                if (CustomCode128) scanFor = scanFor | BarcodeType.CODE_128;
                if (CustomQR) scanFor = scanFor | BarcodeType.QR_CODE;

                if (scanFor == 0) scanFor = BarcodeType.ALL;
            }

            var result = new Result(
                scanFor != ScanFor ? scanFor : null,
                MinValueLengthEdit != MinValueLength ? MinValueLengthEdit : null,
                MaxValueLengthEdit != MaxValueLength ? MaxValueLengthEdit : null
            );

            await ModalInstance.CloseAsync(ModalResult.Ok(result));
        }

        private async Task Cancel() => await ModalInstance.CancelAsync();

        public record Result
        {
            public BarcodeType? ScanFor { get; set; }
            public int? MinValueLength { get; set; }
            public int? MaxValueLength { get; set; }

            public Result(BarcodeType? scanFor, int? minValueLength, int? maxValueLength)
            {
                ScanFor = scanFor;
                MinValueLength = minValueLength;
                MaxValueLength = maxValueLength;
            }
        }
    }
}