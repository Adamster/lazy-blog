using FluentValidation;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.UpdatePost;

public class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Title.MaxLength);

        RuleFor(x => x.Summary).MaximumLength(Summary.MaxLength);
        RuleFor(x => x.Body).NotEmpty();

        RuleFor(x => x.Id).NotEqual(Guid.Empty);
        
        RuleFor(x => x.Tags).NotEmpty();

        RuleFor(x => x.Slug).NotEmpty().MaximumLength(Slug.MaxLength);
    }
}