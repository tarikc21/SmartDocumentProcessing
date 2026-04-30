using Microsoft.EntityFrameworkCore;
using SmartDocumentProcessing.Models;

namespace SmartDocumentProcessing.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<ValidationIssue> ValidationIssues { get; set; }
    }
}