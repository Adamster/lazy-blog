using FluentValidation;

namespace Lazy.Application.Posts.AddPostView;

public class AddPostViewCommandValidator : AbstractValidator<AddPostViewCommand>
{
    public AddPostViewCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}