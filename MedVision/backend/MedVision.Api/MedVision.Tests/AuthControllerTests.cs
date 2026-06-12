using MedVision.Api.Data;
using MedVision.Api.DTOs;
using MedVision.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedVision.Api.Services;
using Xunit;

public class AuthControllerTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Register_NewDoctor_ReturnsOk()
    {
        var context = GetDbContext();

        var controller = new AuthController(context);

        var dto = new RegisterDto
        {
            Email = "doctor@test.com",
            Password = "123456",
            Role = "Doctor",
            VerificationNumber = "DOC-1001"
        };

        var result = await controller.Register(dto);

        Assert.IsType<OkObjectResult>(result);

        Assert.Equal(1, context.Users.Count());

        var user = context.Users.First();

        Assert.Equal("Doctor", user.Role);
        Assert.True(user.IsVerified);
    }

    [Fact]
    public async Task Register_NewStudent_ReturnsOk()
    {
        var context = GetDbContext();

        var controller = new AuthController(context);

        var dto = new RegisterDto
        {
            Email = "student@test.com",
            Password = "123456",
            Role = "Student",
            VerificationNumber = "ST-2025-001"
        };

        var result = await controller.Register(dto);

        Assert.IsType<OkObjectResult>(result);

        var user = context.Users.First();

        Assert.Equal("Student", user.Role);
        Assert.True(user.IsVerified);
    }

    [Fact]
    public async Task Register_ExistingUser_ReturnsBadRequest()
    {
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Email = "test@test.com",
            PasswordHash = "hash"
        });

        await context.SaveChangesAsync();

        var controller = new AuthController(context);

        var dto = new RegisterDto
        {
            Email = "test@test.com",
            Password = "123456",
            Role = "Doctor",
            VerificationNumber = "ST-2025-001"
        };

        var result = await controller.Register(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Email = "test@test.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword("123456"),

            Role = "Doctor",
            IsVerified = true
        });

        await context.SaveChangesAsync();

        var controller = new AuthController(context);

        var dto = new LoginDto
        {
            Email = "test@test.com",
            Password = "123456"
        };

        var result = await controller.Login(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var context = GetDbContext();

        context.Users.Add(new User
        {
            Email = "test@test.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword("123456")
        });

        await context.SaveChangesAsync();

        var controller = new AuthController(context);

        var dto = new LoginDto
        {
            Email = "test@test.com",
            Password = "wrong"
        };

        var result = await controller.Login(dto);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Register_InvalidDoctor_NotVerified()
    {
        var context = GetDbContext();

        var controller = new AuthController(context);

        var dto = new RegisterDto
        {
            Email = "doctor@test.com",
            Password = "123456",
            Role = "Doctor",
            VerificationNumber = "DOC-999999"
        };

        await controller.Register(dto);

        var user = context.Users.First();

        Assert.False(user.IsVerified);
    }

    [Fact]
    public async Task Register_InvalidStudentId_NotVerified()
    {
        var context = GetDbContext();

        var controller = new AuthController(context);

        var dto = new RegisterDto
        {
            Email = "student@test.com",
            Password = "123456",
            Role = "Student",
            VerificationNumber = "ST-999999"
        };

        await controller.Register(dto);

        var user = context.Users.First();

        Assert.False(user.IsVerified);
    }

    [Fact]
    public async Task Register_InvalidDoctorId_NotVerified()
    {
        var context = GetDbContext();

        var controller = new AuthController(context);

        var dto = new RegisterDto
        {
            Email = "doctor@test.com",
            Password = "123456",
            Role = "Doctor",
            VerificationNumber = "DOC-999999"
        };

        await controller.Register(dto);

        var user = context.Users.First();

        Assert.False(user.IsVerified);
    }


}