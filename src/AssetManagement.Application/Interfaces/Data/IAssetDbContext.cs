using Microsoft.EntityFrameworkCore;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Interfaces.Data;

public interface IAssetDbContext
{
    DbSet<Asset> Assets { get; }
    DbSet<Employee> Employees { get; }
    DbSet<AssetAssignment> AssetAssignments { get; }
}

