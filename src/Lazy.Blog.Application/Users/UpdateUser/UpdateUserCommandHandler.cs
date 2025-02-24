using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;
using Lazy.Domain.ValueObjects.User;

namespace Lazy.Application.Users.UpdateUser;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var userNameResult = UserName.Create(request.Username);
        var user = await _userRepository.GetByIdAsync(request.Id, ct);

        if (user == null)
        {
            return Result.Failure(DomainErrors.User.NotFound(request.Id));
        }

        if (await _userRepository.GetByUsernameAsync(userNameResult.Value, ct) is not null)
        {
            return Result.Failure(DomainErrors.UserName.UserNameAlreadyInUse);
        }

        var firstNameResult = FirstName.Create(request.FirstName);
        var lastNameResult = LastName.Create(request.LastName);

        user.ChangeName(
            firstNameResult.Value,
            lastNameResult.Value,
            userNameResult.Value);

        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}