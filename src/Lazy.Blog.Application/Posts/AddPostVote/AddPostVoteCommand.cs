using Lazy.Application.Abstractions.Messaging;
using Lazy.Domain.ValueObjects.Post;

namespace Lazy.Application.Posts.AddPostVote;

public record AddPostVoteCommand(Guid PostId, VoteDirection Direction) : ICommand;