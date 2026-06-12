using MedVision.Api.Data;
using MedVision.Api.DTOs;
using MedVision.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedVision.Api.DTOs;

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
    public async Task<ActionResult<XrayAnalysisResponseDto>> UploadXray(IFormFile file,
   
    [FromForm] int userId,
    [FromForm] string? patientFullName,
    [FromForm] DateTime? patientBirthDate,
    [FromForm] string? patientGender)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Файл не було завантажено.");
        }

        var allowedExtensions = new[] { ".jpg",".jpeg",".png",".dcm",".dicom"};
    

        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Дозволені лише файли JPG, JPEG, PNG або DICOM.");
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

        var detectionsJson =
        System.Text.Json.JsonSerializer.Serialize(
        prediction.Detections );

        var analysis = new XrayAnalysis
        {
            ImagePath = $"/uploads/{uniqueFileName}",
            ResultClass = resultClass,
            Probability = probability,
            Conclusion = conclusion,
            CreatedAt = DateTime.UtcNow,
            HeatmapPath = prediction.HeatmapPath,
            DetectionsJson = detectionsJson,
            PreviewPath = prediction.PreviewPath,
            UserId = userId,
            PatientFullName = patientFullName,

            PatientBirthDate = patientBirthDate,

            PatientGender = patientGender,
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

            CreatedAt = analysis.CreatedAt,

            HeatmapPath = analysis.HeatmapPath,

            DetectionsJson = analysis.DetectionsJson,
            PreviewPath = analysis.PreviewPath,
            Detections = prediction.Detections

        };

        return Ok(response);
    }

    [HttpGet("history/{userId}")]
    public async Task<ActionResult<IEnumerable<XrayAnalysisResponseDto>>> GetHistory(
      int userId)
    {
        var analyses = await _context.XrayAnalyses
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new XrayAnalysisResponseDto
            {
                Id = x.Id,
                ImagePath = x.ImagePath,
                ResultClass = x.ResultClass,
                Probability = x.Probability,
                Conclusion = x.Conclusion,
                CreatedAt = x.CreatedAt,
                HeatmapPath = x.HeatmapPath,
                DetectionsJson = x.DetectionsJson,
                PreviewPath = x.PreviewPath,

                PatientFullName = x.PatientFullName,
                PatientBirthDate = x.PatientBirthDate,
                PatientGender = x.PatientGender
            })
            .ToListAsync();

        return Ok(analyses);
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearHistory()
    {
        var analyses = await _context.XrayAnalyses.ToListAsync();

        _context.XrayAnalyses.RemoveRange(analyses);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Історію очищено"
        });
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
            CreatedAt = analysis.CreatedAt,
            HeatmapPath = analysis.HeatmapPath,
            DetectionsJson = analysis.DetectionsJson,

            PreviewPath = analysis.PreviewPath,

            PatientFullName = analysis.PatientFullName,
            PatientBirthDate = analysis.PatientBirthDate,
            PatientGender = analysis.PatientGender,
        };

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAnalysis(
     int id,
     [FromQuery] int userId)
    {
        var analysis = await _context.XrayAnalyses
            .FindAsync(id);

        if (analysis == null)
        {
            return NotFound();
        }

        if (analysis.UserId != userId)
        {
            return Forbid();
        }

        _context.XrayAnalyses.Remove(analysis);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Аналіз видалено"
        });
    }
}
