using FluentValidation;

namespace Lazy.Application.Identity.Roles.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x=>x.RoleName).NotEmpty().WithMessage("RoleName is required");
    }
}