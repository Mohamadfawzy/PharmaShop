using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Images.Abstractions;


/// <summary>
/// Controls how the output image format is selected.
/// </summary>
public enum ImageOutputFormat
{
    /// <summary>
    /// Automatically choose the best format based on image content (recommended).
    /// </summary>
    Auto = 0,

    Jpeg = 1,
    Png = 2,
    Webp = 3
}