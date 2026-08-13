using Lazy.Domain.Primitives;
using Lazy.Domain.Errors;
using Lazy.Domain.Shared;
namespace Lazy.Domain.Entities;
public sealed class Author:Entity,IAuditableEntity
{
    private Author(){}
    private Author(Guid id,string name):base(id){SetName(name);}
    public string Name{get;private set;}=null!;
    public string NormalizedName{get;private set;}=null!;
    public DateTime CreatedOnUtc{get;set;}
    public DateTime? UpdatedOnUtc{get;set;}
    public static Result<Author> Create(Guid id,string name){var normalized=Normalize(name);return string.IsNullOrWhiteSpace(normalized)||normalized.Length>300?Result.Failure<Author>(DomainErrors.Shelf.InvalidData("Author name must contain 1 to 300 characters")):new Author(id,normalized);}
    public Result Update(string name){var normalized=Normalize(name);if(string.IsNullOrWhiteSpace(normalized)||normalized.Length>300)return Result.Failure(DomainErrors.Shelf.InvalidData("Author name must contain 1 to 300 characters"));SetName(normalized);return Result.Success();}
    private void SetName(string name){Name=Normalize(name);NormalizedName=Name.ToUpperInvariant();}
    private static string Normalize(string value)=>string.Join(' ',value.Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries));
}
