using AssetManagement.Application.DTOs;
using AssetManagement.Application.DTOs.AssetDTOs;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums.Assets;

namespace AssetManagement.Application.Interfaces.Assets;

public interface IAssetService
{
    Task<PagedResultDto<Asset>> GetAllAssetsAsync(AssetQueryDto queryDto);

    Task<Asset> GetAssetByIdOrThrowAsync(int id);

    Task CreateAssetAsync(Asset asset);

    Task CreateAssetsAsync(IEnumerable<Asset> assets);

    Task UpdateAssetAsync(Asset asset);

    Task ChangeAssetStatusAsync(int id, AssetStatus newStatus);

    Task<AssetSummaryDto> GetAssetSummaryServiceAsync(AssetQueryDto queryDto);

    Task<AssetImportValidationResultDto> ValidateImportAsync(List<(int Row, Asset Asset)> assets);
}
