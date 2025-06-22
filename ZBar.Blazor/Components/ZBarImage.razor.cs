using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZBar.Blazor.Interop;

namespace ZBar.Blazor.Components
{
    /// <summary>
    /// A component which scans provided image data for barcode information.
    /// </summary>
    partial class ZBarImage
    {
        [Inject] ImageInterop ImageInterop { get; set; }

        public async Task LoadFromStreamAsync(Stream stream)
        {
            await ImageInterop.LoadFromStreamAsync(stream);
        }
    }
}