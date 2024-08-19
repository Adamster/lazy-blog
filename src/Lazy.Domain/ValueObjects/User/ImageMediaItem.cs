using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.ValueObjects.User;

public class ImageMediaItem : ValueObject
{
    public const int MaxSizeInBytes = 7 * 1024 * 1024; //20 mb
    public const int MaxFilenameLength = 300;
    public const int MaxUrlLength = MaxFilenameLength * 2;
    public static  string[] SupportedFileTypes =
    {
        ".jpg",
        ".png",
        ".jpeg",
        ".gif",
        ".png",
        ".svg",
        ".webp",
        ".avif",
        ".apng"
    };

    private ImageMediaItem()
    {
    }

    private ImageMediaItem(string filename, string url)
    {
        Filename = filename;
        Url = url;
    }

    public string Filename { get; private set; } = null!;
    public string Url { get; private set; } = null!;

    public static Result<ImageMediaItem> Create(string fileName, string url, long fileSizeInBytes, bool ignoreUrlCheck = false)
    {
        var fileExtension = Path.GetExtension(fileName)
            .ToLowerInvariant();

        if (string.IsNullOrEmpty(fileName))
        {
            return Result.Failure<ImageMediaItem>(DomainErrors.ImageMediaItem.EmptyFileName);
        }

        if (!SupportedFileTypes.Contains(fileExtension))
        {
            return Result.Failure<ImageMediaItem>(DomainErrors.ImageMediaItem.NotSupportedExtension);
        }

        if (fileSizeInBytes > MaxSizeInBytes)
        {
            return Result.Failure<ImageMediaItem>(DomainErrors.ImageMediaItem.ImageFileSizeTooLarge);
        }

        if (string.IsNullOrEmpty(url) && !ignoreUrlCheck)
        {
            return Result.Failure<ImageMediaItem>(DomainErrors.ImageMediaItem.EmptyUrl);
        }

        var isValidUrl = Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out _);

        if (!isValidUrl && !ignoreUrlCheck)
        {
            return Result.Failure<ImageMediaItem>(DomainErrors.ImageMediaItem.NotValidUrl);
        }

        return new ImageMediaItem(fileName, url);

    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Url;
    }
}