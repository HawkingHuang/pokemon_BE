using System.ComponentModel.DataAnnotations;

namespace api.Dtos.Account;

public class RegisterDto
{
    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
