using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.Entities;
using Lazy.Domain.Errors;
using Lazy.Domain.Repositories;
using Lazy.Domain.Shared;

namespace Lazy.Application.Tags.UpdateTag;

public class UpdateTagCommandHandler : ICommandHandler<UpdateTagCommand>
{
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTagCommandHandler(
        ITagRepository tagRepository, 
        IUnitOfWork unitOfWork)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateTagCommand request, CancellationToken ct)
    {
        var tag = _tagRepository.GetTagById(request.Id);
        if (tag is null)
        {
            return Result.Failure(DomainErrors.Tag.NotFound(request.Id.ToString()));
        }

        var tagResult = Tag.Create(request.NewTagValue);

        tag.Update(tagResult.Value);
        _tagRepository.Update(tag);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}