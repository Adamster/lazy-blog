using FluentValidation;
using Lazy.Domain.Entities;

namespace Lazy.Application.Tags.UpdateTag;

public class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
    public UpdateTagCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NewTagValue).NotEmpty().MaximumLength(Tag.MaxLength);
    }
}