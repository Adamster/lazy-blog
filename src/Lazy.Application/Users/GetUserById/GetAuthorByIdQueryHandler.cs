using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Users.GetUserById;

internal sealed class GetAuthorByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IUserRepository _userRepository;

    public GetAuthorByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserResponse>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var author = await _userRepository.GetByIdAsync(
            request.UserId,
            cancellationToken);

        if (author is null)
        {
            return Result.Failure<UserResponse>(new Error(
                "Author.NotFound",
                $"The author with Id {request.UserId} was not found."));
        }

        var response = new UserResponse(author.Id, author.Email.Value);

        return response;
    }
}