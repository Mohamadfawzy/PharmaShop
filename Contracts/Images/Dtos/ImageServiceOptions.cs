using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Images.Dtos;

public sealed class ImageServiceOptions
{
    // Variant widths (no upscaling)
    public int MediumWidth { get; set; } = 1000;
    public int SmallWidth { get; set; } = 300;

    // Encoder qualities
    public int JpegQualityOriginal { get; set; } = 85;
    public int JpegQualityMedium { get; set; } = 75;
    public int JpegQualitySmall { get; set; } = 70;

    // If alpha exists: prefer WebP (best) or PNG (safe). Make it configurable.
    public bool PreferWebpWhenAlpha { get; set; } = true;

    // Safety limits
    public long MaxUploadBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
    public int MaxWidth { get; set; } = 10000;
    public int MaxHeight { get; set; } = 10000;
    public long MaxPixels { get; set; } = 60_000_000; // 60 MP

    // Folder names under root
    public string OriginalFolder { get; set; } = "original";
    public string MediumFolder { get; set; } = "medium";
    public string SmallFolder { get; set; } = "small";

    // Naming
    public int MaxPrefixLength { get; set; } = 40;

    // Atomic save
    public string TempFolderName { get; set; } = "_tmp";
}