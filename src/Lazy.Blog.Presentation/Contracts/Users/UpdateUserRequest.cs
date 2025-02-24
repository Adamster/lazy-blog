using System.ComponentModel.DataAnnotations;

namespace Lazy.Presentation.Contracts.Users;

public record UpdateUserRequest(
   [Required] string FirstName, 
   [Required] string LastName,
   [Required] string UserName);