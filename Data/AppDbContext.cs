using Microsoft.EntityFrameworkCore;
using Multiverse.Web.Models;

namespace Multiverse.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ClientDocument> ClientDocuments => Set<ClientDocument>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasConversion<int>();
        });

        b.Entity<Client>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.Phone);
            e.Property(x => x.Status).HasConversion<int>();
            e.Property(x => x.State).HasConversion<int>();
            e.Property(x => x.ClientType).HasConversion<int>();
            e.Property(x => x.InterestedIn).HasConversion<int?>();
            e.Property(x => x.PreferredPropertyType).HasConversion<int?>();
            e.Property(x => x.BHK).HasConversion<int?>();
            e.Property(x => x.Possession).HasConversion<int?>();
            e.Property(x => x.LoanRequired).HasConversion<int?>();
            e.Property(x => x.LeadSource).HasConversion<int?>();
            e.Property(x => x.LeadStatus).HasConversion<int>();
            e.Property(x => x.Priority).HasConversion<int>();

            e.HasOne(x => x.AssignedEmployee)
            .WithMany()
            .HasForeignKey(x => x.AssignedEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);
        });
        b.Entity<ClientDocument>(e =>
        {
            e.Property(x => x.Type).HasConversion<int>();
            e.HasOne(x => x.Client)
            .WithMany(c => c.Documents)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.ClientId, x.Type });
        });
    }
}