using Lazy.Domain.Primitives;
using Lazy.Domain.Errors;
using Lazy.Domain.Shared;

namespace Lazy.Domain.Entities;

public sealed class BookComment : Entity, IAuditableEntity
{
    public const int MaxLength = 4000;
    private BookComment() { }
    private BookComment(Guid id, Guid bookId, Guid userId, string body) : base(id)
        => (BookId, UserId, Body) = (bookId, userId, body);
    public Guid BookId { get; private set; }
    public Book Book { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public DateTime CreatedOnUtc { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public static Result<BookComment> Create(Guid id,Guid bookId,Guid userId,string body)=>Valid(body)?new BookComment(id,bookId,userId,body):Result.Failure<BookComment>(DomainErrors.Shelf.InvalidData($"Comment must contain 1 to {MaxLength} characters"));
    public Result Update(string body){if(!Valid(body))return Result.Failure(DomainErrors.Shelf.InvalidData($"Comment must contain 1 to {MaxLength} characters"));Body=body;return Result.Success();}
    private static bool Valid(string body)=>!string.IsNullOrWhiteSpace(body)&&body.Length<=MaxLength;
}
