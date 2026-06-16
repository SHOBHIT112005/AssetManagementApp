using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly AssetDbContext _context;

    public AssetRepository(AssetDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Asset>> GetAllAsync()
    {
        return await _context.Assets
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Asset?> GetByIdAsync(int id)
    {
        return await _context.Assets.FindAsync(id);
    }

    public async Task<IEnumerable<Asset>> GetAvailableAssetsAsync()
    {
        return await _context.Assets.AsNoTracking().Where(asset => asset.Status == AssetStatus.Available).ToListAsync();
    }

    public async Task AddAsync(Asset asset)
    {
        await _context.Assets.AddAsync(asset);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Asset asset)
    {
        _context.Assets.Update(asset);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var asset = await GetByIdAsync(id);
        if (asset is null)
            return;

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();
    }
}
