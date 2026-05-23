using MedVision.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MedVision.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<XrayAnalysis> XrayAnalyses => Set<XrayAnalysis>();
}