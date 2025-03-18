using Lazy.Application.Abstractions.Messaging;

namespace Lazy.Application.Identity.Roles.CreateRole;

public record CreateRoleCommand(string RoleName) : ICommand;
