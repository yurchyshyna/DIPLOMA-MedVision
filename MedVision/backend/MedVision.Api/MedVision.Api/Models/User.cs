namespace MedVision.Api.Models;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<XrayAnalysis> Analyses { get; set; }
    = new List<XrayAnalysis>();

    public string Role { get; set; } = "Student";

    public bool IsVerified { get; set; }

    public string? VerificationNumber { get; set; }
}