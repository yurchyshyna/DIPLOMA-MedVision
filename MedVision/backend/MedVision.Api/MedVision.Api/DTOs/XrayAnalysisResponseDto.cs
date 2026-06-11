namespace MedVision.Api.DTOs;

public class XrayAnalysisResponseDto
{
    public int Id { get; set; }

    public string ImagePath { get; set; } = string.Empty;

    public string ResultClass { get; set; } = string.Empty;

    public double Probability { get; set; }

    public string Conclusion { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? HeatmapPath { get; set; }

    public string? DetectionsJson { get; set; }

    public List<DetectionDto> Detections { get; set; } = new();
    public string? PreviewPath { get; set; }
}