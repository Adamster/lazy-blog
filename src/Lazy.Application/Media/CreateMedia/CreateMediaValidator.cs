using FluentValidation;

namespace Lazy.Application.Media.CreateMedia;

public class CreateMediaValidator : AbstractValidator<CreateMediaCommand>
{
    public CreateMediaValidator()
    {
        RuleFor(x => x.File).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.File.Length).NotEqual(0);
        RuleFor(x => x.File.Name).NotEmpty();
    }
}