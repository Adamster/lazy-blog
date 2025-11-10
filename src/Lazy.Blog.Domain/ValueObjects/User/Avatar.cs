using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.ValueObjects.User;

public class Avatar : ValueObject
{
    public const int MaxSizeInBytes = 50 * 1024 * 1024; //30 mb
    public const int MaxFilenameLength = 300;
    public const int MaxUrlLength = MaxFilenameLength * 2;
    private static readonly string[] SupportedFileTypes =
    [
        ".jpg",
        ".png",
        ".jpeg",
        ".gif",
        ".png",
        ".svg",
        ".webp",
        ".avif",
        ".apng"
    ];

    private Avatar()
    {
    }

    private Avatar(string filename, string url)
    {
        Filename = filename;
        Url = url;
    }

    public string? Filename { get; private set; }
    public string? Url { get; private set; }

    public static Result<Avatar> Create(string fileName, string url, long fileSizeInBytes, bool ignoreUrlCheck = false)
    {
        var fileExtension = Path.GetExtension(fileName)
            .ToLowerInvariant();

        if (string.IsNullOrEmpty(fileName))
        {
            return Result.Failure<Avatar>(DomainErrors.Avatar.EmptyFileName);
        }

        if (!SupportedFileTypes.Contains(fileExtension))
        {
            return Result.Failure<Avatar>(DomainErrors.Avatar.NotSupportedExtension);
        }

        if (fileSizeInBytes > MaxSizeInBytes)
        {
            return Result.Failure<Avatar>(DomainErrors.Avatar.ImageFileSizeTooLarge);
        }

        if (string.IsNullOrEmpty(url) && !ignoreUrlCheck)
        {
            return Result.Failure<Avatar>(DomainErrors.Avatar.EmptyUrl);
        }

        var isValidUrl = Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out _);

        if (!isValidUrl && !ignoreUrlCheck)
        {
            return Result.Failure<Avatar>(DomainErrors.Avatar.NotValidUrl);
        }

        return new Avatar(fileName, url);

    }

    public void Clear()
    {
        Url = null;
        Filename = null;
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Url ?? string.Empty;
    }
}