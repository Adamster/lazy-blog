﻿using FluentValidation;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.CreatePost;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Title.MaxLength);

        RuleFor(x => x.Summary).MaximumLength(Summary.MaxLength);

        RuleFor(x => x.Tags).NotEmpty();

        RuleFor(x => x.Body).NotEmpty();
    }
}