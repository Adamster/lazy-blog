using System.Security.Cryptography;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Lazy.Application.Abstractions.File;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace Lazy.Infrastructure.Storage;

public sealed class BookCoverService(IOptions<AzureBlobStorageOptions> options, ILogger<BookCoverService> logger) : IBookCoverService
{
    private const long MaxInputBytes = 20 * 1024 * 1024;
    private readonly AzureBlobStorageOptions _options = options.Value;
    public async Task<BookCoverUpload?> ProcessAndUploadAsync(FileUpload file, CancellationToken ct)
    {
        if (file.Length is <= 0 or > MaxInputBytes) return null;
        try
        {
            await using var input = file.OpenReadStream();
            using var memory = new MemoryStream();
            await input.CopyToAsync(memory, ct);
            using var data = SKData.CreateCopy(memory.ToArray());
            using var codec = SKCodec.Create(data);
            if (codec is null || (long)codec.Info.Width * codec.Info.Height > 40_000_000) return null;
            var encodedFormat = codec.EncodedFormat;
            if (encodedFormat is not (SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Webp or SKEncodedImageFormat.Png)) return null;
            using var decoded = SKBitmap.Decode(data);
            if (decoded is null) return null;
            using var oriented = ApplyOrientation(decoded, codec.EncodedOrigin);
            var targetHeight = Math.Min(1000, oriented.Height);
            var targetWidth = targetHeight == oriented.Height ? oriented.Width :
                Math.Max(1, (int)Math.Round(oriented.Width * (targetHeight / (double)oriented.Height)));
            using var processed = targetHeight == oriented.Height ? oriented.Copy() : Resize(oriented, targetWidth, targetHeight);
            using var image = SKImage.FromBitmap(processed);
            var quality = encodedFormat == SKEncodedImageFormat.Png ? 100 : 92;
            using var encoded = image.Encode(encodedFormat, quality);
            if (encoded is null) return null;
            var bytes = encoded.ToArray();
            var hash = Convert.ToHexStringLower(SHA256.HashData(bytes));
            var ext = encodedFormat == SKEncodedImageFormat.Webp ? "webp" : encodedFormat == SKEncodedImageFormat.Png ? "png" : "jpg";
            var contentType = encodedFormat == SKEncodedImageFormat.Webp ? "image/webp" : encodedFormat == SKEncodedImageFormat.Png ? "image/png" : "image/jpeg";
            var path = $"shelf/covers/{hash[..16]}.{ext}";
            var blob = Container().GetBlobClient(path);
            if ((await blob.ExistsAsync(ct)).Value)
            {
                var properties = (await blob.GetPropertiesAsync(cancellationToken: ct)).Value;
                if (!properties.Metadata.TryGetValue("sha256", out var storedHash) || storedHash != hash) return null;
            }
            else
            {
                await using var upload = new MemoryStream(bytes);
                await blob.UploadAsync(upload, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                    Metadata = new Dictionary<string, string> { ["sha256"] = hash },
                    Conditions = new BlobRequestConditions { IfNoneMatch = ETag.All }
                }, ct);
            }
            return new BookCoverUpload(blob.Uri.AbsoluteUri, hash, processed.Width, processed.Height, contentType, file.FileName);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or RequestFailedException or NotSupportedException)
        { logger.LogWarning(ex, "Invalid or failed Shelf cover {Name}", file.FileName); return null; }
    }

    private static SKBitmap Resize(SKBitmap source, int width, int height)
    {
        var result = new SKBitmap(new SKImageInfo(width, height, source.ColorType, source.AlphaType, source.ColorSpace));
        using var canvas = new SKCanvas(result);
        canvas.Clear(SKColors.Transparent);
        canvas.DrawBitmap(source, new SKRect(0, 0, width, height),
            new SKSamplingOptions(SKCubicResampler.Mitchell), null);
        return result;
    }

    private static SKBitmap ApplyOrientation(SKBitmap source, SKEncodedOrigin origin)
    {
        var swapsAxes = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var result = new SKBitmap(new SKImageInfo(swapsAxes ? source.Height : source.Width,
            swapsAxes ? source.Width : source.Height, source.ColorType, source.AlphaType, source.ColorSpace));
        using var canvas = new SKCanvas(result);
        canvas.Clear(SKColors.Transparent);
        canvas.Translate(result.Width / 2f, result.Height / 2f);
        switch (origin)
        {
            case SKEncodedOrigin.TopRight: canvas.Scale(-1, 1); break;
            case SKEncodedOrigin.BottomRight: canvas.RotateDegrees(180); break;
            case SKEncodedOrigin.BottomLeft: canvas.Scale(1, -1); break;
            case SKEncodedOrigin.LeftTop: canvas.RotateDegrees(90); canvas.Scale(1, -1); break;
            case SKEncodedOrigin.RightTop: canvas.RotateDegrees(90); break;
            case SKEncodedOrigin.RightBottom: canvas.RotateDegrees(90); canvas.Scale(-1, 1); break;
            case SKEncodedOrigin.LeftBottom: canvas.RotateDegrees(270); break;
        }
        canvas.DrawBitmap(source, -source.Width / 2f, -source.Height / 2f,
            new SKSamplingOptions(SKFilterMode.Linear), null);
        return result;
    }

    private BlobContainerClient Container() => new BlobServiceClient(
        new Uri($"https://{_options.AccountName}.blob.core.windows.net"), new DefaultAzureCredential())
        .GetBlobContainerClient(_options.ImageContainerName);
}
