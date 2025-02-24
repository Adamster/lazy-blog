using FluentValidation;

namespace Lazy.Application.Users.UploadUserAvatar;

public class UploadUserAvatarValidator : AbstractValidator<UploadUserAvatarCommand>
{
    public UploadUserAvatarValidator()
    {
        RuleFor(x => x.File).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.File.Length).NotEqual(0);
        RuleFor(x => x.File.Name).NotEmpty();
    }
}