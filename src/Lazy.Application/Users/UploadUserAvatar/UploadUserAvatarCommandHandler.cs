using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Shared;

namespace Lazy.Application.Users.UploadUserAvatar;

public class UploadUserAvatarCommandHandler : ICommandHandler<UploadUserAvatarCommand>
{
    public Task<Result> Handle(UploadUserAvatarCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}