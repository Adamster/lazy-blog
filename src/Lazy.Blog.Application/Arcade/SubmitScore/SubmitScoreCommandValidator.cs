using FluentValidation;
using Lazy.Domain.ValueObjects.Arcade;

namespace Lazy.Application.Arcade.SubmitScore;

public class SubmitScoreCommandValidator : AbstractValidator<SubmitScoreCommand>
{
    private const int MaxScore = 1_000_000;

    public SubmitScoreCommandValidator()
    {
        RuleFor(x => x.Game).NotEmpty().MaximumLength(GameKey.MaxLength);

        RuleFor(x => x.Score).InclusiveBetween(0, MaxScore);
    }
}
