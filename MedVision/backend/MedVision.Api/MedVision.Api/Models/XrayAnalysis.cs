namespace MedVision.Api.Models
{
    public class XrayAnalysis
    {
        public int Id { get; set; }


        public int? UserId { get; set; }

        public User? User { get; set; }

        public string? PatientFullName { get; set; }

        public DateTime? PatientBirthDate { get; set; }

        public string? PatientGender { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public string ResultClass { get; set; } = string.Empty;

        public double Probability { get; set; }

        public string Conclusion { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? HeatmapPath { get; set; }

        public string? DetectionsJson { get; set; }

        public string? PreviewPath { get; set; }
    }
}
