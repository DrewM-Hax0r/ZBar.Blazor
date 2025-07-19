# ZBar.Blazor
ZBar.Blazor provides easy to use razor components that allow developers to add fully featured barcode scanning capabilities to their Blazor WASM applications in minutes. This project uses [zbar-wasm](https://github.com/undecaf/zbar-wasm) which compiles [zbar](https://github.com/mchehab/zbar) into native web assembly. This allows developers to utalize the client's web browser to efficiently scan image data for barcode information. ZBar.Blazor is simple to use but flexible and powerful, providing access to all of ZBar's features through a Razor/C# API and without the need to write any JavaScript code.

## Barcode Support
ZBar.Blazor supports scanning the following types of barcodes:
 - UPC-A, UPC-E
 - ISBN-13, ISBN-10
 - EAN-13, EAN-8, EAN-5, EAN-2
 - I25
 - GS1 DataBar
 - GS1 DataBar Expanded
 - Code 128, Code 93, Code 39
 - QR Code

## Quickstart
1. Include the Zbar.Blazor library as a dependency or install through NuGet to your Blazor client application project.

2. Reference the ZBar module loader script in your Blazor client application's index html page:
```
<script src="_content/ZBar.Blazor/zbar.js"></script>
```
This script reference should be placed below the blazor.webassembly.js module loader script.

3. Add the ZBarImage or ZBarCamera component to any page in your Blazor client application and bind a function to the OnBarcodesFound event:
```
@using ZBar.Blazor.Dtos

@*Use ZBarImage to scan image files for barcode information*@
<ZBarImage OnBarcodesFound="FoundBarcodes" />

@*Use ZBarCamera to scan a video feed for barcode information*@
<ZBarCamera OnBarcodesFound="FoundBarcodes" />

@code {

    private void FoundBarcodes(ScanResult scanResult)
    {
        // Scanned barcode information is accessable in the ScanResult object
    }

}
```
Refer to the sandbox demo for details on how to provide image sources to ZBarImage and video feeds to ZBarCamera.

## Version Table
Refer to the table below to see which versions of [zbar-wasm](https://github.com/undecaf/zbar-wasm) and [zbar](https://github.com/mchehab/zbar) are used by this project.

| ZBar.Blazor | zbar-wasm | zbar     |
| ----------- | --------- | -------- |
| v1.0.0      | v0.11.0   | v0.23.93 |
