using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _assetRepository;

    public AssetService(IAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public Task<IEnumerable<Asset>> GetAllAssetsAsync()
    {
        return _assetRepository.GetAllAsync();
    }

    public async Task<Asset?> GetAssetByIdAsync(int id)
    {
        var asset = await _assetRepository.GetByIdAsync(id) ?? throw new ArgumentException("Asset not found.");
        return asset;
    }

    public async Task CreateAssetAsync(Asset asset)
    {
        if (string.IsNullOrWhiteSpace(asset.AssetName))
        {
            throw new ArgumentException("Asset name is required.");
        }
        if (string.IsNullOrWhiteSpace(asset.SerialNumber))
        {
            throw new ArgumentException("Serial number is required.");
        }
        asset.Status = AssetStatus.Available;
        await _assetRepository.AddAsync(asset);
    }

    public async Task UpdateAssetAsync(Asset asset)
    {
        var existingAsset = await _assetRepository.GetByIdAsync(asset.Id);
        if (existingAsset == null)
        {
            throw new ArgumentException("Not a valid asset Id, asset not found.");
        }

        if (string.IsNullOrWhiteSpace(asset.AssetName))
        {
            throw new ArgumentException("Asset name is required.");
        }
        if (string.IsNullOrWhiteSpace(asset.SerialNumber))
        {
            throw new ArgumentException("Serial number is required.");
        }
        existingAsset.AssetName = asset.AssetName;
        existingAsset.Type = asset.Type;
        existingAsset.SerialNumber = asset.SerialNumber;
        existingAsset.PurchaseDate = asset.PurchaseDate;
        existingAsset.WarrantyExpiryDate = asset.WarrantyExpiryDate;
        existingAsset.Status = asset.Status;
        existingAsset.Condition = asset.Condition;
        await _assetRepository.UpdateAsync(existingAsset);
    }

    public async Task ChangeAssetStatusAsync(int id,AssetStatus newStatus)
    {
        var asset = await _assetRepository.GetByIdAsync(id);
        if (asset == null)
        {
            throw new ArgumentException("Not a valid asset Id, asset not found.");
        }
        
        asset.Status = newStatus;
        await _assetRepository.UpdateAsync(asset);
    }
}