namespace MedVision.Api.DTOs;

public class DetectionDto
{
    public string ClassName { get; set; } = string.Empty;

    public double Confidence { get; set; }

    public string Description { get; set; } = string.Empty;
}