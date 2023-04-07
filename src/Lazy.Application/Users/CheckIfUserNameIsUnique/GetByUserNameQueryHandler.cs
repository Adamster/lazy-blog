using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Application.Users.CheckIfUserNameIsUnique;

public class GetByUserNameQueryHandler : IQueryHandler<CheckIfUserNameIsUnique, bool>
{

    private readonly IUserRepository _userRepository;

    public GetByUserNameQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<bool>> Handle(CheckIfUserNameIsUnique request, CancellationToken cancellationToken)
    {
        var userNameResult = UserName.Create(request.Username);
        var user = await _userRepository.GetByUsernameAsync(userNameResult.Value, cancellationToken);

        if (user is not null)
        {
            return Result.Failure<bool>(DomainErrors.UserName.UserNameAlreadyInUse);
        }

        return Result.Success(true);
    }
}