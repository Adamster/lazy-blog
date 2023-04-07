using FluentValidation;
using Lazy.Domain.Entities;

namespace Lazy.Application.Comments.UpdateComment;

public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();

        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.CommentText).NotEmpty().MaximumLength(Comment.MaxLength);
    }
}