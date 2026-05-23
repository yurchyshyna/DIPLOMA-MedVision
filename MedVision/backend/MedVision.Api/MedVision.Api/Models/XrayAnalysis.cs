namespace MedVision.Api.Models
{
    public class XrayAnalysis
    {
        public int Id { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public string ResultClass { get; set; } = string.Empty;

        public double Probability { get; set; }

        public string Conclusion { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
