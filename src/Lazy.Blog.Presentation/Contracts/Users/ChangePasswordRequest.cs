namespace Lazy.Presentation.Contracts.Users;

public record ChangePasswordRequest(string OldPassword, string NewPassword);