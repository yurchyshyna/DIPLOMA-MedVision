using MedVision.Api.Controllers;
using MedVision.Api.Data;
using MedVision.Api.DTOs;
using MedVision.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

public class XrayAnalysisControllerTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private XrayAnalysisController CreateController(AppDbContext context)
    {
        var env = new Mock<IWebHostEnvironment>();

        var httpFactory =
            new Mock<IHttpClientFactory>();

        return new XrayAnalysisController(
            context,
            env.Object,
            httpFactory.Object);
    }

    [Fact]
    public async Task GetHistory_ReturnsOk()
    {
        var context = GetDbContext();

        context.XrayAnalyses.Add(new XrayAnalysis
        {
            ResultClass = "Normal",
            UserId = 1
        });

        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result =
            await controller.GetHistory(1);

        Assert.IsType<
            OkObjectResult>(
            result.Result);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsOk()
    {
        var context = GetDbContext();

        var analysis = new XrayAnalysis
        {
            ResultClass = "Normal",
            UserId = 1
        };

        context.XrayAnalyses.Add(analysis);

        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result =
            await controller.GetById(
                analysis.Id);

        Assert.IsType<
            OkObjectResult>(
            result.Result);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        var context = GetDbContext();

        var controller =
            CreateController(context);

        var result =
            await controller.GetById(999);

        Assert.IsType<
            NotFoundObjectResult>(
            result.Result);
    }

    [Fact]
    public async Task ClearHistory_RemovesAllRecords()
    {
        var context = GetDbContext();

        context.XrayAnalyses.Add(
            new XrayAnalysis
            {
                UserId = 1
            });

        context.XrayAnalyses.Add(
            new XrayAnalysis
            {
                UserId = 1
            });

        await context.SaveChangesAsync();

        var controller =
            CreateController(context);

        await controller.ClearHistory();

        Assert.Empty(
            context.XrayAnalyses);
    }

    [Fact]
    public async Task DeleteAnalysis_RemovesRecord()
    {
        var context = GetDbContext();

        var analysis = new XrayAnalysis
        {
            ResultClass = "Normal",
            UserId = 1
        };

        context.XrayAnalyses.Add(analysis);

        await context.SaveChangesAsync();

        var controller =
            CreateController(context);

        await controller.DeleteAnalysis(
            analysis.Id,
            1);

        Assert.Empty(
            context.XrayAnalyses);
    }

    [Fact]
    public async Task DeleteAnalysis_ForeignUser_ReturnsForbid()
    {
        var context = GetDbContext();

        var analysis = new XrayAnalysis
        {
            ResultClass = "Normal",
            UserId = 1
        };

        context.XrayAnalyses.Add(analysis);

        await context.SaveChangesAsync();

        var controller =
            CreateController(context);

        var result =
            await controller.DeleteAnalysis(
                analysis.Id,
                2);

        Assert.IsType<ForbidResult>(
            result);
    }

    [Fact]
    public async Task GetHistory_ReturnsOnlyCurrentUserRecords()
    {
        var context = GetDbContext();

        context.XrayAnalyses.Add(
            new XrayAnalysis
            {
                UserId = 1,
                ResultClass = "Normal"
            });

        context.XrayAnalyses.Add(
            new XrayAnalysis
            {
                UserId = 2,
                ResultClass = "Abnormal"
            });

        await context.SaveChangesAsync();

        var controller =
            CreateController(context);

        var result =
            await controller.GetHistory(1);

        var okResult =
            Assert.IsType<
                OkObjectResult>(
                result.Result);

        var analyses =
            Assert.IsAssignableFrom<
                IEnumerable<XrayAnalysisResponseDto>>(
                    okResult.Value);

        Assert.Single(analyses);
    }
}