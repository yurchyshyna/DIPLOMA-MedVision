using MedVision.Api.Controllers;
using MedVision.Api.Data;
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
            ResultClass = "Normal"
        });

        await context.SaveChangesAsync();

        var controller = CreateController(context);

        var result =
            await controller.GetHistory();

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
            ResultClass = "Normal"
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
            new XrayAnalysis());

        context.XrayAnalyses.Add(
            new XrayAnalysis());

        await context.SaveChangesAsync();

        var controller =
            CreateController(context);

        await controller.ClearHistory();

        Assert.Empty(
            context.XrayAnalyses);
    }
}