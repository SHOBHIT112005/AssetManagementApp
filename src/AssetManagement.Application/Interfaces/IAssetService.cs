using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.Interfaces;

public interface IAssetService
{
    Task<IEnumerable<Asset>> GetAllAssetsAsync();

    Task<Asset?> GetAssetByIdAsync(int id);

    Task CreateAssetAsync(Asset asset);

    Task UpdateAssetAsync(Asset asset);

    Task ChangeAssetStatusAsync(int id, AssetStatus newStatus);
}