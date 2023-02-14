using FluentValidation;
using Lazy.Domain.ValueObjects;

namespace Lazy.Application.Users.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(FirstName.MaxLength);

        RuleFor(x => x.LastName).NotEmpty().MaximumLength(LastName.MaxLength);

        RuleFor(x => x.Id).NotEqual(Guid.Empty);
    }
}