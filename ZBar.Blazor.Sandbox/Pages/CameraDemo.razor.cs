using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using ZBar.Blazor.Components;
using ZBar.Blazor.Config;
using ZBar.Blazor.Dtos;
using ZBar.Blazor.Sandbox.Components;

namespace ZBar.Blazor.Sandbox.Pages
{
    public partial class CameraDemo
    {
        [CascadingParameter] public IModalService Modal { get; set; }

        private ZBarCamera Camera;

        private bool AutoScan { get; set; } = true;
        private int AutoScanInterval { get; set; } = 1000;
        private BarcodeType ScanFor { get; set; } = BarcodeType.UPC_A;
        private bool Verbose { get; set; } = true;
        private CameraViewType CameraView { get; set; } = CameraViewType.VideoFeed;

        private HardwareDevice[] AvailableHardwareDevices;
        private string SelectedHardwareDeviceId;

        private Barcode[] FoundBarcodes;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                AvailableHardwareDevices = await Camera.GetAvailableDevices();
                StateHasChanged();
            }
        }

        private async Task StartVideoFeed()
        {
            if (SelectedHardwareDeviceId != null)
            {
                await Camera.StartVideoFeed(SelectedHardwareDeviceId);
            }
        }

        private async Task EndVideoFeed()
        {
            await Camera.EndVideoFeed();
        }

        private async Task Scan()
        {
            await Camera.Scan();
        }

        private async Task OpenAdvancedConfig()
        {
            var parameters = new ModalParameters()
                .Add(nameof(AdvancedConfiguration.ScanFor), ScanFor);

            var modal = Modal.Show<AdvancedConfiguration>("Advanced Configuration", parameters);
            var result = await modal.Result;

            if(!result.Cancelled) {
                var config = result.Data as AdvancedConfiguration.Result;
                if (config.ScanFor.HasValue) ScanFor = config.ScanFor.Value;
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

        private void OnBarcodesFound(ScanResult scanResult)
        {
            FoundBarcodes = scanResult.Barcodes;
            StateHasChanged();
        }
    }
}