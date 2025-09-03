using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service;
public class ImageServiceOptions
{
    public int MediumSize { get; set; } = 1000;
    public int SmallSize { get; set; } = 300;
    public int OriginalQuality { get; set; } = 85;
    public int MediumQuality { get; set; } = 75;
    public int SmallQuality { get; set; } = 70;
    public int MaxFileNameLength { get; set; } = 100;
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp" };
    public string[] AllowedMimeTypes { get; set; } = { "image/jpeg", "image/png", "image/webp", "image/gif", "image/bmp" };
}