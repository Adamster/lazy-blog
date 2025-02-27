namespace Lazy.Presentation.Contracts.Users;

public record ResetPasswordRequest(string Token, string Email, string NewPassword);