using System.ComponentModel.DataAnnotations;

namespace LanAnhComputer.Dtos;

public class RegisterDto
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [RegularExpression(@"^\d+$", ErrorMessage = "Phone must contain digits only.")]
    public string? Phone { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public string Role { get; set; } = null!;
    public long UserId { get; set; }

    public string FullName { get; set; } = string.Empty;
}

public class CurrentUserDto
{
    public long UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}
