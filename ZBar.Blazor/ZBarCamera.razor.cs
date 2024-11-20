using Microsoft.AspNetCore.Components;

namespace ZBar.Blazor
{
    partial class ZBarCamera
    {
        [Inject] CameraInterop CameraInterop { get; set; }

        [Parameter] public int Width { get; set; } = 320;
        [Parameter] public int Height { get; set; } = 240;

        private ElementReference Video;
        private ElementReference Canvas;
    }
}