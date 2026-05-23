using MedVision.Api.Data;
using MedVision.Api.DTOs;
using MedVision.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedVision.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class XrayAnalysisController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpClientFactory _httpClientFactory;

    public XrayAnalysisController(
        AppDbContext context,
        IWebHostEnvironment environment,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _environment = environment;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<XrayAnalysisResponseDto>> UploadXray(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Файл не було завантажено.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Дозволені лише файли JPG, JPEG або PNG.");
        }

        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Тимчасова імітація AI-аналізу
        //var random = new Random();
        //var probability = Math.Round(random.NextDouble() * 100, 2);

        //var resultClass = probability > 50 ? "Pathology detected" : "Normal";
        //var conclusion = probability > 50
        //    ? "На знімку виявлено ознаки можливої патології легень."
        //    : "Ознак патології легень не виявлено.";

        var client = _httpClientFactory.CreateClient();

        await using var imageStream = System.IO.File.OpenRead(filePath);

        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(imageStream);

        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

        content.Add(fileContent, "file", uniqueFileName);

        var aiResponse = await client.PostAsync(
            "http://127.0.0.1:8000/predict",
            content);

        if (!aiResponse.IsSuccessStatusCode)
        {
            return StatusCode(500, "Помилка під час звернення до AI-сервісу.");
        }

        var prediction = await aiResponse.Content.ReadFromJsonAsync<AiPredictionResponse>();

        if (prediction == null)
        {
            return StatusCode(500, "AI-сервіс не повернув результат.");
        }

        var probability = prediction.Probability;
        var resultClass = prediction.ResultClass;
        var conclusion = prediction.Conclusion;

        var analysis = new XrayAnalysis
        {
            ImagePath = $"/uploads/{uniqueFileName}",
            ResultClass = resultClass,
            Probability = probability,
            Conclusion = conclusion,
            CreatedAt = DateTime.UtcNow
        };

        _context.XrayAnalyses.Add(analysis);
        await _context.SaveChangesAsync();

        var response = new XrayAnalysisResponseDto
        {
            Id = analysis.Id,
            ImagePath = analysis.ImagePath,
            ResultClass = analysis.ResultClass,
            Probability = analysis.Probability,
            Conclusion = analysis.Conclusion,
            CreatedAt = analysis.CreatedAt
        };

        return Ok(response);
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<XrayAnalysisResponseDto>>> GetHistory()
    {
        var analyses = await _context.XrayAnalyses
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new XrayAnalysisResponseDto
            {
                Id = x.Id,
                ImagePath = x.ImagePath,
                ResultClass = x.ResultClass,
                Probability = x.Probability,
                Conclusion = x.Conclusion,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(analyses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<XrayAnalysisResponseDto>> GetById(int id)
    {
        var analysis = await _context.XrayAnalyses.FindAsync(id);

        if (analysis == null)
        {
            return NotFound("Аналіз не знайдено.");
        }

        var response = new XrayAnalysisResponseDto
        {
            Id = analysis.Id,
            ImagePath = analysis.ImagePath,
            ResultClass = analysis.ResultClass,
            Probability = analysis.Probability,
            Conclusion = analysis.Conclusion,
            CreatedAt = analysis.CreatedAt
        };

        return Ok(response);
    }
}

public class AiPredictionResponse
{
    public string ResultClass { get; set; } = string.Empty;

    public double Probability { get; set; }

    public string Conclusion { get; set; } = string.Empty;
}