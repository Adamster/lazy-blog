using FluentValidation;

namespace Lazy.Application.Posts.GetPublishedPosts;

public class GetPublishedPostsQueryValidator : AbstractValidator<GetPublishedPostsQuery>
{
    public GetPublishedPostsQueryValidator()
    {
        RuleFor(x => x.Offset).GreaterThanOrEqualTo(0); 
    }
}