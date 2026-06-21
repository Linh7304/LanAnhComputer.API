using LanAnhComputer.API.Dtos;
using LanAnhComputer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LanAnhComputer.API.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDtos>> GetProfile()
    {
        var userId = long.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (user == null)
            return NotFound();

        return Ok(new UserProfileDtos
        {
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth
        });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
    {
        var userId = long.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (user == null)
            return NotFound();

        user.FullName = dto.FullName;
        user.Phone = dto.Phone;
        user.Gender = dto.Gender;
        user.DateOfBirth = dto.DateOfBirth;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            return BadRequest("Mật khẩu xác nhận không khớp.");

        var userId = long.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (user == null)
            return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(
            dto.CurrentPassword,
            user.PasswordHash))
        {
            return BadRequest("Mật khẩu hiện tại không đúng.");
        }

        user.PasswordHash =
            BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return Ok();
    }
}