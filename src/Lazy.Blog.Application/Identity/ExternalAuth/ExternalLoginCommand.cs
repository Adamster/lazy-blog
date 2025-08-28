using Lazy.Application.Abstractions.Messaging;
using Lazy.Application.Users.Login;

namespace Lazy.Application.Identity.ExternalAuth;

public sealed record ExternalLoginCommand(string Email, string Provider, string ProviderUserId) : ICommand<LoginResponse>;