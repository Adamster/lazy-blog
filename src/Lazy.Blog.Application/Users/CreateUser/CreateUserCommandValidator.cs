using FluentValidation;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Application.Users.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty();

        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(DisplayName.MaxLength);
    }
}
