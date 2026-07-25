using AssetManagement.Application.Interfaces.Data;
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
