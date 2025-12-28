using Contracts.Images.Abstractions;
using Contracts.Images.Dtos;
using Contracts.Images.Enums;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Images;



public sealed class ImageSharpProcessor : IImageProcessor
{
    private readonly Contracts.Images.Dtos.ImageServiceOptions _opt;

    public ImageSharpProcessor(IOptions<Contracts.Images.Dtos.ImageServiceOptions> options)
    {
        _opt = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<ProcessedImage> ProcessAsync(Stream input, CancellationToken ct)
    {
        // IMPORTANT: input is assumed to be a safe, size-limited stream (buffered by ImageService).
        input.Position = 0;

        using var image = await LoadAndValidateAsync(input, ct);

        // Determine if alpha exists (to avoid breaking transparency).
        var hasAlpha = HasAlpha(image);

        var format = ChooseFormat(hasAlpha);

        // Produce variants (bytes) with no upscaling and stripped metadata.
        var result = new Dictionary<ImageVariant, byte[]>(3)
        {
            [ImageVariant.Original] = EncodeVariant(image, targetWidth: image.Width, variant: ImageVariant.Original, format: format),
            [ImageVariant.Medium] = EncodeVariant(image, targetWidth: _opt.MediumWidth, variant: ImageVariant.Medium, format: format),
            [ImageVariant.Small] = EncodeVariant(image, targetWidth: _opt.SmallWidth, variant: ImageVariant.Small, format: format)
        };

        return new ProcessedImage
        {
            Format = format,
            OriginalWidth = image.Width,
            OriginalHeight = image.Height,
            VariantBytes = result
        };
    }

    private async Task<Image> LoadAndValidateAsync(Stream stream, CancellationToken ct)
    {
        try
        {
            // Detect format and decode.
            //IImageFormat? detected;
            var image = await Image.LoadAsync(stream, ct);

            // Guard against dangerous sizes.
            if (image.Width <= 0 || image.Height <= 0)
            {
                image.Dispose();
                throw new ArgumentException("Invalid image dimensions.");
            }

            if (image.Width > _opt.MaxWidth || image.Height > _opt.MaxHeight)
            {
                image.Dispose();
                throw new ArgumentException($"Image dimensions exceed allowed limit ({_opt.MaxWidth}x{_opt.MaxHeight}).");
            }

            var pixels = (long)image.Width * image.Height;
            if (pixels > _opt.MaxPixels)
            {
                image.Dispose();
                throw new ArgumentException($"Image pixels exceed allowed limit ({_opt.MaxPixels}).");
            }

            return image;
        }
        catch (UnknownImageFormatException)
        {
            throw new ArgumentException("Unsupported image format.");
        }
        catch (InvalidImageContentException)
        {
            throw new ArgumentException("Corrupted or invalid image content.");
        }
    }

    private StoredImageFormat ChooseFormat(bool hasAlpha)
    {
        if (!hasAlpha)
            return StoredImageFormat.Jpeg;

        // With alpha: prefer WebP (smaller) or fallback to PNG (lossless).
        return _opt.PreferWebpWhenAlpha ? StoredImageFormat.Webp : StoredImageFormat.Png;
    }

    private static bool HasAlpha(Image image)
    {
        // NOTE: ImageSharp provides metadata about pixel type.
        // This is a pragmatic check: if the image has an alpha channel it is likely to be present.
        return image.Metadata?.GetPngMetadata() != null
               || image.Metadata?.GetWebpMetadata() != null
               || image.PixelType.AlphaRepresentation != PixelAlphaRepresentation.None;
    }

    private byte[] EncodeVariant(Image original, int targetWidth, ImageVariant variant, StoredImageFormat format)
    {
        // 1) Compute target size (no upscaling).
        var width = original.Width;
        var height = original.Height;

        if (targetWidth > 0 && width > targetWidth)
        {
            var ratio = (double)targetWidth / width;
            width = targetWidth;
            height = (int)Math.Round(height * ratio);
        }

        // 2) Clone and resize.
        using var resized = original.Clone(ctx =>
        {
            // ResizeMode.Max keeps aspect ratio and fits in the specified size.
            ctx.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3
            });
        });

        // 3) Strip sensitive metadata.
        resized.Metadata.ExifProfile = null;
        resized.Metadata.IccProfile = null;
        resized.Metadata.XmpProfile = null;

        // 4) Encode to memory.
        using var ms = new MemoryStream(capacity: 256 * 1024);

        switch (format)
        {
            case StoredImageFormat.Jpeg:
                resized.Save(ms, new JpegEncoder
                {
                    Quality = variant switch
                    {
                        ImageVariant.Original => _opt.JpegQualityOriginal,
                        ImageVariant.Medium => _opt.JpegQualityMedium,
                        ImageVariant.Small => _opt.JpegQualitySmall,
                        _ => _opt.JpegQualityMedium
                    }
                });
                break;

            case StoredImageFormat.Png:
                // NOTE: PNG is lossless, quality setting is not like JPEG.
                resized.Save(ms, new PngEncoder());
                break;

            case StoredImageFormat.Webp:
                resized.Save(ms, new WebpEncoder());
                break;

            default:
                throw new InvalidOperationException("Unsupported output format.");
        }

        return ms.ToArray();
    }
}