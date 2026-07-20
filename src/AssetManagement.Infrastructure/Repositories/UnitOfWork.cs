using System.Threading.Tasks;
using AssetManagement.Application.Interfaces.Assets;
using AssetManagement.Application.Interfaces.Assignments;
using AssetManagement.Application.Interfaces.Employees;
using AssetManagement.Application.Interfaces.Data;
using AssetManagement.Application.Interfaces.Agent;
using AssetManagement.Infrastructure.Data;

namespace AssetManagement.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AssetDbContext _context;

    public UnitOfWork(AssetDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

