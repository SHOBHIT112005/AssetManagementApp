using AssetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AssetManagement.Application.Interfaces.Assets;
using AssetManagement.Application.Interfaces.Assignments;
using AssetManagement.Application.Interfaces.Employees;
using AssetManagement.Application.Interfaces.Data;
using AssetManagement.Application.Interfaces.Agent;

namespace AssetManagement.Infrastructure.Data;

public class AssetDbContext : IdentityDbContext<ApplicationUser>, IAssetDbContext
{
    public AssetDbContext( DbContextOptions<AssetDbContext> options ) : base(options)
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
