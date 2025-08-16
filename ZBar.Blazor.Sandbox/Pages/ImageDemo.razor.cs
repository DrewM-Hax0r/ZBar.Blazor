using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ZBar.Blazor.Components;
using ZBar.Blazor.Config;
using ZBar.Blazor.Dtos;
using ZBar.Blazor.Sandbox.Components;

namespace ZBar.Blazor.Sandbox.Pages
{
    partial class ImageDemo
    {
        private const long MAX_FILESIZE_BYTES = 10240000;

        [CascadingParameter] public IModalService Modal { get; set; }

        private ZBarImage Image;
        private Barcode[] FoundBarcodes;
        private bool ImageLoadFailed;
        private bool ScanEnabled;

        private ImageSourceType SourceType { get; set; } = ImageSourceType.Stream;
        private string ImageSrc { get; set; } = "";
        private bool AutoScan { get; set; } = true;
        private BarcodeType ScanFor { get; set; } = BarcodeType.UPC_A;
        private int MinValueLength { get; set; } = 0;
        private int MaxValueLength { get; set; } = 0;
        private bool HonorCheckDigit { get; set; } = true;
        private bool IncludeCheckDigit { get; set; } = true;
        private bool EnableFullCharacterSet { get; set; } = true;

        private bool Verbose { get; set; } = true;
        private ImageViewType ImageView { get; set; } = ImageViewType.ImageFeed;

        private async Task LoadImage(InputFileChangeEventArgs args)
        {
            if (args.FileCount == 1)
            {
                using var stream = args.File.OpenReadStream(MAX_FILESIZE_BYTES);
                await Image.LoadFromStreamAsync(stream);
            }
        }

        private void ImageLoadSuccess()
        {
            ImageLoadFailed = false;
            ScanEnabled = true;
            FoundBarcodes = null;
        }

        private void ImageLoadFailure()
        {
            ImageLoadFailed = true;
            ScanEnabled = false;
            FoundBarcodes = null;
        }

        private void OnBarcodesFound(ScanResult scanResult)
        {
            FoundBarcodes = scanResult.Barcodes;
            StateHasChanged();
        }

        private void OnBarcodesNotFound()
        {
            FoundBarcodes = null;
            StateHasChanged();
        }

        private async Task Scan()
        {
            await Image.Scan();
        }

        private async Task OpenAdvancedConfig()
        {
            var parameters = new ModalParameters()
                .Add(nameof(AdvancedConfiguration.ScanFor), ScanFor)
                .Add(nameof(AdvancedConfiguration.MinValueLength), MinValueLength)
                .Add(nameof(AdvancedConfiguration.MaxValueLength), MaxValueLength)
                .Add(nameof(AdvancedConfiguration.HonorCheckDigit), HonorCheckDigit)
                .Add(nameof(AdvancedConfiguration.IncludeCheckDigit), IncludeCheckDigit)
                .Add(nameof(AdvancedConfiguration.EnableFullCharacterSet), EnableFullCharacterSet);

            var modal = Modal.Show<AdvancedConfiguration>("Advanced Configuration", parameters);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                var config = result.Data as AdvancedConfiguration.Result;
                if (config.ScanFor.HasValue) ScanFor = config.ScanFor.Value;
                if (config.MinValueLength.HasValue) MinValueLength = config.MinValueLength.Value;
                if (config.MaxValueLength.HasValue) MaxValueLength = config.MaxValueLength.Value;
                if (config.HonorCheckDigit.HasValue) HonorCheckDigit = config.HonorCheckDigit.Value;
                if (config.IncludeCheckDigit.HasValue) IncludeCheckDigit = config.IncludeCheckDigit.Value;
                if (config.EnableFullCharacterSet.HasValue) EnableFullCharacterSet = config.EnableFullCharacterSet.Value;
            }
        }

        private string GetScanForDisplay()
        {
            if (ScanFor == BarcodeType.ALL) return "All Types";

            var flagsCount = 0;
            foreach (var type in Enum.GetValues<BarcodeType>())
                if (ScanFor.HasFlag(type)) flagsCount++;

            if (flagsCount > 1) return "Multiple Types";
            if (ScanFor.HasFlag(BarcodeType.UPC_A)) return "UPC-A";
            if (ScanFor.HasFlag(BarcodeType.UPC_E)) return "UPC-E";
            if (ScanFor.HasFlag(BarcodeType.ISBN_13)) return "ISBN-13";
            if (ScanFor.HasFlag(BarcodeType.ISBN_10)) return "ISBN-10";
            if (ScanFor.HasFlag(BarcodeType.EAN_13)) return "EAN-13";
            if (ScanFor.HasFlag(BarcodeType.EAN_8)) return "EAN-8";
            if (ScanFor.HasFlag(BarcodeType.EAN_5)) return "EAN-5";
            if (ScanFor.HasFlag(BarcodeType.EAN_2)) return "EAN-2";
            if (ScanFor.HasFlag(BarcodeType.I25)) return "I25";
            if (ScanFor.HasFlag(BarcodeType.DATABAR)) return "GS1 DataBar";
            if (ScanFor.HasFlag(BarcodeType.DATABAR_EXPANDED)) return "GS1 DataBar (Expanded)";
            if (ScanFor.HasFlag(BarcodeType.CODE_39)) return "Code-39";
            if (ScanFor.HasFlag(BarcodeType.CODE_93)) return "Code-93";
            if (ScanFor.HasFlag(BarcodeType.CODE_128)) return "Code-128";
            if (ScanFor.HasFlag(BarcodeType.QR_CODE)) return "QR Code";

            return "N/A";
        }
    }
}