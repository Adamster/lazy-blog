using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Tags.UpdateTag;

public record UpdateTagCommand(Guid Id, string NewTagValue) : ICommand;