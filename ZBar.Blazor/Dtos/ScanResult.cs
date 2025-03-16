namespace ZBar.Blazor.Dtos
{
    public class ScanResult
    {
        public Barcode[] Barcodes { get; }

        internal ScanResult(Barcode[] barcodes)
        {
            Barcodes = barcodes;
        }
    }
}