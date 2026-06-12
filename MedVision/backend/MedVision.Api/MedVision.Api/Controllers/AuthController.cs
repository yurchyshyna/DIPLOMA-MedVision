using MedVision.Api.Data;
using MedVision.Api.DTOs;
using MedVision.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedVision.Api.Services;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterDto dto)
    {
        var exists = await _context.Users
            .AnyAsync(x => x.Email == dto.Email);

        if (exists)
        {
            return BadRequest("User exists");
        }

        bool verified = false;

        if (dto.Role == "Doctor")
        {
            verified =
                VerificationRegistry.DoctorIds
                    .Contains(dto.VerificationNumber);
        }

        if (dto.Role == "Student")
        {
            verified =
                VerificationRegistry.StudentIds
                    .Contains(dto.VerificationNumber);
        }

        var user = new User
        {
            Email = dto.Email,

            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    dto.Password),

            Role = dto.Role,

            VerificationNumber =
                dto.VerificationNumber,

            IsVerified = verified
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Registration successful",
            role = user.Role,
            verified = user.IsVerified
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(
                x => x.Email == dto.Email);

        if (user == null)
        {
            return Unauthorized();
        }

        var valid =
            BCrypt.Net.BCrypt.Verify(
                dto.Password,
                user.PasswordHash);

        if (!valid)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            user.Id,
            user.Email,
            user.Role,
            user.IsVerified
        });
    }
}