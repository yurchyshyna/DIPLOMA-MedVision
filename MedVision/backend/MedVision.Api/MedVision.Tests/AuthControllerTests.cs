using MedVision.Api.Data;
using MedVision.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
    public async Task Register_NewUser_ReturnsOk()
    {
        var context = GetDbContext();

        var controller = new AuthController(context);

        var dto = new RegisterDto
        {
            Email = "test@test.com",
            Password = "123456"
        };

        var result = await controller.Register(dto);

        Assert.IsType<OkResult>(result);

        Assert.Equal(1, context.Users.Count());
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
            Password = "123456"
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
                BCrypt.Net.BCrypt.HashPassword("123456")
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
}