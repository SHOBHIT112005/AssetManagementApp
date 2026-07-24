using AssetManagement.Application.DTOs;
using AssetManagement.Application.DTOs.AssetDTOs;
using AssetManagement.Application.Interfaces.Assets;
using AssetManagement.Application.Interfaces.Data;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums.Assets;
using AssetManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace AssetManagement.Infrastructure.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly AssetDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public AssetRepository(AssetDbContext context, ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
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
    }

    public Task UpdateAsync(Asset asset)
    {
        _context.Assets.Update(asset);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var asset = await GetByIdAsync(id);
        if (asset is null)
            return;

        _context.Assets.Remove(asset);
    }

    public async Task<AssetSummaryDto> GetAssetSummaryAsync(AssetQueryDto queryDto)
    {
        var sql = @" 
             SELECT 
                 COUNT(*) AS TotalAssets, 
                 COALESCE(SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END),0) AS AvailableAssets, 
                 COALESCE(SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END),0) AS AssignedAssets, 
                 COALESCE(SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END),0) AS UnderRepairAssets, 
                 COALESCE(SUM(CASE WHEN Status = 4 THEN 1 ELSE 0 END),0) AS RetiredAssets 
                 FROM Assets
                 WHERE (@Status IS NULL OR Status = @Status)
                 AND (@AssetType IS NULL OR Type = @AssetType)";

        using var connection = _sqlConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync<dynamic>(sql, new
        {
            queryDto.Status,
            queryDto.AssetType
        });
        return new AssetSummaryDto
        {
            TotalAssets = result.FirstOrDefault()?.TotalAssets ?? 0,
            AvailableAssets = result.FirstOrDefault()?.AvailableAssets ?? 0,
            AssignedAssets = result.FirstOrDefault()?.AssignedAssets ?? 0,
            UnderRepairAssets = result.FirstOrDefault()?.UnderRepairAssets ?? 0,
            RetiredAssets = result.FirstOrDefault()?.RetiredAssets ?? 0
        };
    }
    public async Task<HashSet<string>> GetExistingSerialNumbersAsync(IEnumerable<string> serialNumbers)
    {
        var serialSet = serialNumbers
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim().ToLowerInvariant())
            .ToHashSet();

        return await _context.Assets
            .AsNoTracking()
            .Where(a => serialSet.Contains(a.SerialNumber.ToLower()))
            .Select(a => a.SerialNumber.ToLower())
            .ToHashSetAsync();
    }
}


