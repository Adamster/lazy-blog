using FluentValidation;
using Lazy.Domain.Entities;

namespace Lazy.Application.Comments.AddComment;

public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.PostId).NotEmpty();

        RuleFor(x => x.CommentText).NotEmpty().MaximumLength(Comment.MaxLength);
    }
}