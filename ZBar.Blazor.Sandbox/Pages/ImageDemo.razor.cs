using Microsoft.AspNetCore.Components.Forms;
using ZBar.Blazor.Components;
using ZBar.Blazor.Dtos;

namespace ZBar.Blazor.Sandbox.Pages
{
    partial class ImageDemo
    {
        private const long MAX_FILESIZE_BYTES = 10240000;

        private ZBarImage Image;
        private Barcode[] FoundBarcodes;
        private bool ImageLoadFailed;
        private bool ScanEnabled;
        private bool Verbose { get; set; } = true;

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
    }
}