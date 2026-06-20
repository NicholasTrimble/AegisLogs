using Microsoft.EntityFrameworkCore;
using AegisLogs.Api.Models;

namespace AegisLogs.Api;

public class AegisDbContext : DbContext
{
    public AegisDbContext(DbContextOptions<AegisDbContext> options) : base(options)
    {
    }

    public DbSet<AuditEvent> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AuditEvent>().HasKey(e => e.EventId);
    }
}