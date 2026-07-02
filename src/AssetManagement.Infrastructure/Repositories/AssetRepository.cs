using AssetManagement.Application.DTOs;
using AssetManagement.Application.DTOs.AssetDTOs;
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

    public async Task<PagedResultDto<Asset>> GetAllAsync(AssetQueryDto queryDto)
    {
        var query = _context.Assets.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(queryDto.SearchTerm))
        {
            switch (queryDto.SearchField)
            {
                case AssetSearchField.AssetName:
                    query = query.Where(a => a.AssetName.Contains(queryDto.SearchTerm));
                    break;
                case AssetSearchField.SerialNumber:
                    query = query.Where(a => a.SerialNumber.Contains(queryDto.SearchTerm));
                    break;
                default:
                    break;
            }
        }

        if (queryDto.Status.HasValue)
        {
            query = query.Where(a => a.Status == queryDto.Status.Value);
        }

        if (queryDto.AssetType.HasValue)
        {
            query = query.Where(a => a.Type == queryDto.AssetType.Value);
        }
        if (!queryDto.SortField.HasValue)
        {
            query = query.OrderBy(a => a.AssetName);
        }
        else
        {
            switch (queryDto.SortField.Value)
            {
                case AssetSortField.AssetName:
                    query = queryDto.SortDirection == SortDirection.Ascending ? query.OrderBy(a => a.AssetName) : query.OrderByDescending(a => a.AssetName);
                    break;
                case AssetSortField.Status:
                    query = queryDto.SortDirection == SortDirection.Ascending ? query.OrderBy(a => a.Status) : query.OrderByDescending(a => a.Status);
                    break;
                case AssetSortField.PurchaseDate:
                    query = queryDto.SortDirection == SortDirection.Ascending ? query.OrderBy(a => a.PurchaseDate) : query.OrderByDescending(a => a.PurchaseDate);
                    break;
                case AssetSortField.WarrantyExpiryDate:
                    query = queryDto.SortDirection == SortDirection.Ascending ? query.OrderBy(a => a.WarrantyExpiryDate) : query.OrderByDescending(a => a.WarrantyExpiryDate);
                    break;
                default:
                    break;
            }
        }
        var totalCount = await query.CountAsync();
        var assets = await query.Skip((queryDto.PageNumber - 1) * queryDto.PageSize).Take(queryDto.PageSize).ToListAsync();
        return new PagedResultDto<Asset>
        {
            Items = assets,
            TotalCount = totalCount,
            PageNumber = queryDto.PageNumber,
            PageSize = queryDto.PageSize
        };
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

    //Important function that provides a summary of assets based on their status and type, useful for dashboard displays and quick insights.
    public async Task<AssetSummaryDto> GetAssetSummaryAsync(AssetQueryDto queryDto)
    {
        var query = _context.Assets.AsNoTracking().AsQueryable();

        if (queryDto.Status.HasValue)
        {
            query = query.Where(a => a.Status == queryDto.Status.Value);
        }

        if (queryDto.AssetType.HasValue)
        {
            query = query.Where(a => a.Type == queryDto.AssetType.Value);
        }

        var summary = await query
        .GroupBy(a => a.Status)
        .Select(g => new { Status = g.Key, Count = g.Count() })
        .ToDictionaryAsync(x => x.Status, x => x.Count);

        return new AssetSummaryDto
        {
            TotalAssets = summary.Values.Sum(),
            AvailableAssets = summary.GetValueOrDefault(AssetStatus.Available, 0),
            AssignedAssets = summary.GetValueOrDefault(AssetStatus.Assigned, 0),
            UnderRepairAssets = summary.GetValueOrDefault(AssetStatus.UnderRepair, 0),
            RetiredAssets = summary.GetValueOrDefault(AssetStatus.Retired, 0)
        };
    }
}
