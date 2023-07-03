namespace Lazy.Presentation.Contracts.Users;

public sealed record RefreshTokenRequest(string AccessToken, string RefreshToken);