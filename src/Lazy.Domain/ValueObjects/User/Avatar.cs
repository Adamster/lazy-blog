using Lazy.Domain.Errors;
using Lazy.Domain.Primitives;
using Lazy.Domain.Shared;

namespace Lazy.Domain.ValueObjects.User;

public class Avatar : ValueObject
{
    public const int MaxSizeInBytes = 20 * 1024 * 1024; //20 mb
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

    private Avatar()
    {
    }

    private Avatar(string filename, string url)
    {
        Filename = filename;
        Url = url;
    }

    public string Filename { get; private set; } = null!;
    public string Url { get; private set; } = null!;

    public static Result<Avatar> Create(string fileName, string url)
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

        if (string.IsNullOrEmpty(url))
        {
            return Result.Failure<Avatar>(DomainErrors.Avatar.EmptyUrl);
        }

        var isValidUrl = Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out _);

        if (!isValidUrl)
        {
            return Result.Failure<Avatar>(DomainErrors.Avatar.NotValidUrl);
        }

        return new Avatar(fileName, url);

    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Url;
    }
}