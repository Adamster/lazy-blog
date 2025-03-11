using FluentValidation;

namespace Lazy.Application.Posts.GetPostByTag;

public class GetPostsByTagQueryValidator : AbstractValidator<GetPostByTagQuery>
{
    public GetPostsByTagQueryValidator()
    {
        RuleFor(x => x.Offset).GreaterThanOrEqualTo(0); 
    }
}