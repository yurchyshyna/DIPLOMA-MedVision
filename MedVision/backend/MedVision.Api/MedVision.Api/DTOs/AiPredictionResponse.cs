namespace MedVision.Api.DTOs;

public class AiPredictionResponse
{
    public string ResultClass { get; set; } = string.Empty;

    public double Probability { get; set; }

    public string Conclusion { get; set; } = string.Empty;

    public string HeatmapPath { get; set; } = string.Empty;

    public string PreviewPath { get; set; }

    public List<DetectionDto> Detections { get; set; } = new();



   
}