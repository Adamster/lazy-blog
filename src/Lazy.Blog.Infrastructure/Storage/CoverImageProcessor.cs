using System.Security.Cryptography;
using Lazy.Application.Abstractions.File;
using SkiaSharp;

namespace Lazy.Infrastructure.Storage;

public sealed record ProcessedCover(
    byte[] Bytes,
    string Sha256,
    int Width,
    int Height,
    string ContentType,
    string Extension);

public interface ICoverImageProcessor
{
    Task<ProcessedCover?> ProcessAsync(FileUpload file, CancellationToken ct);
}

public sealed class CoverImageProcessor : ICoverImageProcessor
{
    private const long MaxInputBytes = 20 * 1024 * 1024;
    private const long MaxPixels = 40_000_000;
    private const int MaxHeight = 1000;

    public async Task<ProcessedCover?> ProcessAsync(FileUpload file, CancellationToken ct)
    {
        if (file.Length is <= 0 or > MaxInputBytes)
        {
            return null;
        }

        await using var input = file.OpenReadStream();
        using var memory = new MemoryStream();
        await input.CopyToAsync(memory, ct);
        byte[] inputBytes = memory.ToArray();

        return await Task.Run(() => Decode(inputBytes), ct);
    }

    private static ProcessedCover? Decode(byte[] inputBytes)
    {
        using var data = SKData.CreateCopy(inputBytes);
        using var codec = SKCodec.Create(data);
        if (codec is null || (long)codec.Info.Width * codec.Info.Height > MaxPixels)
        {
            return null;
        }

        if (!TryGetOutput(codec.EncodedFormat, out string extension, out string contentType, out int quality))
        {
            return null;
        }

        using var decoded = SKBitmap.Decode(data);
        if (decoded is null)
        {
            return null;
        }

        using var oriented = ApplyOrientation(decoded, codec.EncodedOrigin);
        int targetHeight = Math.Min(MaxHeight, oriented.Height);
        int targetWidth = targetHeight == oriented.Height
            ? oriented.Width
            : Math.Max(1, (int)Math.Round(oriented.Width * (targetHeight / (double)oriented.Height)));
        using var processed = targetHeight == oriented.Height
            ? oriented.Copy()
            : Resize(oriented, targetWidth, targetHeight);
        using var image = SKImage.FromBitmap(processed);
        using var encoded = image.Encode(codec.EncodedFormat, quality);
        if (encoded is null)
        {
            return null;
        }

        byte[] bytes = encoded.ToArray();
        string hash = Convert.ToHexStringLower(SHA256.HashData(bytes));
        return new ProcessedCover(bytes, hash, processed.Width, processed.Height, contentType, extension);
    }

    private static bool TryGetOutput(
        SKEncodedImageFormat format,
        out string extension,
        out string contentType,
        out int quality)
    {
        (extension, contentType, quality) = format switch
        {
            SKEncodedImageFormat.Jpeg => ("jpg", "image/jpeg", 92),
            SKEncodedImageFormat.Webp => ("webp", "image/webp", 92),
            SKEncodedImageFormat.Png => ("png", "image/png", 100),
            _ => (string.Empty, string.Empty, 0)
        };
        return extension.Length > 0;
    }

    private static SKBitmap Resize(SKBitmap source, int width, int height)
    {
        var result = new SKBitmap(
            new SKImageInfo(width, height, source.ColorType, source.AlphaType, source.ColorSpace));
        using var canvas = new SKCanvas(result);
        canvas.Clear(SKColors.Transparent);
        canvas.DrawBitmap(
            source,
            new SKRect(0, 0, width, height),
            new SKSamplingOptions(SKCubicResampler.Mitchell),
            null);
        return result;
    }

    private static SKBitmap ApplyOrientation(SKBitmap source, SKEncodedOrigin origin)
    {
        bool swapsAxes = origin is
            SKEncodedOrigin.LeftTop or
            SKEncodedOrigin.RightTop or
            SKEncodedOrigin.RightBottom or
            SKEncodedOrigin.LeftBottom;
        var result = new SKBitmap(new SKImageInfo(
            swapsAxes ? source.Height : source.Width,
            swapsAxes ? source.Width : source.Height,
            source.ColorType,
            source.AlphaType,
            source.ColorSpace));
        using var canvas = new SKCanvas(result);
        canvas.Clear(SKColors.Transparent);
        canvas.Translate(result.Width / 2f, result.Height / 2f);
        ApplyTransform(canvas, origin);
        canvas.DrawBitmap(
            source,
            -source.Width / 2f,
            -source.Height / 2f,
            new SKSamplingOptions(SKFilterMode.Linear),
            null);
        return result;
    }

    private static void ApplyTransform(SKCanvas canvas, SKEncodedOrigin origin)
    {
        switch (origin)
        {
            case SKEncodedOrigin.TopRight:
                canvas.Scale(-1, 1);
                break;
            case SKEncodedOrigin.BottomRight:
                canvas.RotateDegrees(180);
                break;
            case SKEncodedOrigin.BottomLeft:
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.LeftTop:
                canvas.RotateDegrees(90);
                canvas.Scale(1, -1);
                break;
            case SKEncodedOrigin.RightTop:
                canvas.RotateDegrees(90);
                break;
            case SKEncodedOrigin.RightBottom:
                canvas.RotateDegrees(90);
                canvas.Scale(-1, 1);
                break;
            case SKEncodedOrigin.LeftBottom:
                canvas.RotateDegrees(270);
                break;
        }
    }
}
