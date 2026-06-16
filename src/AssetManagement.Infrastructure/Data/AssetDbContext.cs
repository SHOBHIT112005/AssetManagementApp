using AssetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Data;

public class AssetDbContext : DbContext
{
    public AssetDbContext( DbContextOptions<AssetDbContext> options ) : base(options)
    {
        
    }

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Asset> Assets => Set<Asset>();
}