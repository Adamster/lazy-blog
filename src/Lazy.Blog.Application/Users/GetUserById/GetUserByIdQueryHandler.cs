using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Users.GetUserById;

internal sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserBookStatusRepository _userBookStatuses;

    public GetUserByIdQueryHandler(IUserRepository userRepository, IUserBookStatusRepository userBookStatuses)
    {
        _userRepository = userRepository;
        _userBookStatuses = userBookStatuses;
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

        int readBooksCount = await _userBookStatuses.GetCountAsync(user.UserName!, ReadingStatus.Read, ct);
        return new UserResponse(user) with { ReadBooksCount = readBooksCount };
    }
}