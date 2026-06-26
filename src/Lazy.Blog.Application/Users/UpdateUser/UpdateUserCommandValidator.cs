using FluentValidation;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Application.Users.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(DisplayName.MaxLength);

        RuleFor(x => x.Username).NotEmpty().MaximumLength(UserName.MaxLength);

        RuleFor(x => x.Id).NotEqual(Guid.Empty);
    }
}
