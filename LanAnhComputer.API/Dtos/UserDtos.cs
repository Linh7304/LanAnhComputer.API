using System.ComponentModel.DataAnnotations;

namespace LanAnhComputer.Dtos;

public class UserDto
{
    public long UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string Role { get; set; } = null!;
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? AddressLine { get; set; }
    public string? Ward { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserUpsertDto
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
    public string PasswordHash { get; set; } = null!;

    [Required]
    public string Role { get; set; } = "Customer";
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? AddressLine { get; set; }
    public string? Ward { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateUserRoleDto
{
    [Required]
    public string Role { get; set; } = "Customer";
}

public class UpdateUserActiveDto
{
    public bool IsActive { get; set; }
}
