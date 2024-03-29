using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Users.GetUserById;

internal sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserResponse>> Handle(
        GetUserByIdQuery request,
        CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(
            request.UserId,
            ct);

        if (user is null)
        {
            return Result.Failure<UserResponse>(new Error(
                "Author.NotFound",
                $"The author with Id {request.UserId} was not found."));
        }

        var response = new UserResponse(user);
        return response;
    }
}