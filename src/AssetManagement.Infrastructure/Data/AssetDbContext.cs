using AssetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AssetManagement.Application.Interfaces.Data;

namespace AssetManagement.Infrastructure.Data;

public class AssetDbContext : IdentityDbContext<ApplicationUser>, IAssetDbContext
{
    public AssetDbContext(DbContextOptions<AssetDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Asset> Assets => Set<Asset>();

    public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Asset>()
            .HasIndex(a => a.SerialNumber)
            .IsUnique();

        builder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();
    }
}
