using AssetManagement.Application.DTOs;
using AssetManagement.Application.DTOs.AssetDTOs;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Interfaces;

public interface IAssetRepository
{
    Task<PagedResultDto<Asset>> GetAllAsync(AssetQueryDto queryDto);

    Task<Asset?> GetByIdAsync(int id);

    Task<IEnumerable<Asset>> GetAvailableAssetsAsync();

    Task AddAsync(Asset asset);

    Task UpdateAsync(Asset asset);

    Task DeleteAsync(int id); //change to soft delete in future to keep asset history

    Task<AssetSummaryDto> GetAssetSummaryAsync(AssetQueryDto queryDto);
}



