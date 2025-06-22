using Microsoft.AspNetCore.Components.Forms;
using ZBar.Blazor.Components;

namespace ZBar.Blazor.Sandbox.Pages
{
    partial class ImageDemo
    {
        private const long MAX_FILESIZE_BYTES = 10240000;

        private ZBarImage Image;

        private async Task LoadImage(InputFileChangeEventArgs args)
        {
            if (args.FileCount == 1)
            {
                using var stream = args.File.OpenReadStream(MAX_FILESIZE_BYTES);
                await Image.LoadFromStreamAsync(stream);
            }
        }
    }
}
