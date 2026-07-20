using AssetManagement.Application.DTOs;
using AssetManagement.Application.DTOs.AssetDTOs;
using AssetManagement.Application.Interfaces.Assets;
using AssetManagement.Application.Interfaces.Assignments;
using AssetManagement.Application.Interfaces.Employees;
using AssetManagement.Application.Interfaces.Data;
using AssetManagement.Application.Interfaces.Agent;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums.Assets;
using AssetManagement.Domain.Enums.Assignments;
using AssetManagement.Domain.Enums.Employees;
using Microsoft.Extensions.Logging;

namespace AssetManagement.Application.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AssetService> _logger;

    public AssetService(IAssetRepository assetRepository, IUnitOfWork unitOfWork, ILogger<AssetService> logger)
    {
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<PagedResultDto<Asset>> GetAllAssetsAsync(AssetQueryDto queryDto)
    {
        try
        {
            if (queryDto.PageNumber < 1)
                queryDto.PageNumber = 1;

            if (queryDto.PageSize < 1)
                queryDto.PageSize = 10;

            return _assetRepository.GetAllAsync(queryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset list");
            throw;
        }
    }

    public async Task<Asset> GetAssetByIdOrThrowAsync(int id)
    {
        try
        {
            var asset = await _assetRepository.GetByIdAsync(id) ?? throw new ArgumentException("Asset not found.");
            return asset;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching asset with Id {AssetId}", id);
            throw;
        }
    }

    public async Task CreateAssetAsync(Asset asset)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(asset.AssetName))
                throw new ArgumentException("Asset name is required.");
            if (string.IsNullOrWhiteSpace(asset.SerialNumber))
                throw new ArgumentException("Serial number is required.");
            if (asset.WarrantyExpiryDate <= asset.PurchaseDate)
                throw new ArgumentException("Warranty expiry date must be strictly after the purchase date.");

            var existingSerials = await _assetRepository.GetExistingSerialNumbersAsync(new[] { asset.SerialNumber });
            if (existingSerials.Contains(asset.SerialNumber.ToLowerInvariant()))
                throw new ArgumentException("Serial number already exists.");

            asset.Status = AssetStatus.Available;
            await _assetRepository.AddAsync(asset);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new asset");
            throw;
        }
    }

    public async Task CreateAssetsAsync(IEnumerable<Asset> assets)
    {
        try
        {
            foreach (var asset in assets)
            {
                if (string.IsNullOrWhiteSpace(asset.AssetName))
                    throw new ArgumentException("Asset name is required.");
                if (string.IsNullOrWhiteSpace(asset.SerialNumber))
                    throw new ArgumentException("Serial number is required.");
                if (asset.WarrantyExpiryDate <= asset.PurchaseDate)
                    throw new ArgumentException("Warranty expiry date must be strictly after the purchase date.");

                asset.Status = AssetStatus.Available;
                await _assetRepository.AddAsync(asset);
            }
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk-creating assets");
            throw;
        }

    }

    public async Task UpdateAssetAsync(Asset asset)
    {
        try
        {
            var existingAsset = await _assetRepository.GetByIdAsync(asset.Id);
            if (existingAsset == null)
                throw new ArgumentException("Not a valid asset Id, asset not found.");

            if (string.IsNullOrWhiteSpace(asset.AssetName))
                throw new ArgumentException("Asset name is required.");
            if (string.IsNullOrWhiteSpace(asset.SerialNumber))
                throw new ArgumentException("Serial number is required.");
            if (asset.WarrantyExpiryDate <= asset.PurchaseDate)
                throw new ArgumentException("Warranty expiry date must be strictly after the purchase date.");

            var existingSerials = await _assetRepository.GetExistingSerialNumbersAsync(new[] { asset.SerialNumber });
            if (existingSerials.Contains(asset.SerialNumber.ToLowerInvariant()) &&
                !string.Equals(existingAsset.SerialNumber, asset.SerialNumber, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Serial number already exists.");

            if (existingAsset.Status == AssetStatus.Assigned && asset.Status != AssetStatus.Assigned)
                throw new ArgumentException("Cannot change status of an Assigned asset. Please return it from the Assignments page first.");
                
            if (existingAsset.Status != AssetStatus.Assigned && asset.Status == AssetStatus.Assigned)
                throw new ArgumentException("Cannot manually change status to Assigned. Please assign it to an employee via the Assignments page.");

            existingAsset.AssetName = asset.AssetName;
            existingAsset.Type = asset.Type;
            existingAsset.SerialNumber = asset.SerialNumber;
            existingAsset.PurchaseDate = asset.PurchaseDate;
            existingAsset.WarrantyExpiryDate = asset.WarrantyExpiryDate;
            existingAsset.Status = asset.Status;
            existingAsset.Condition = asset.Condition;

            await _assetRepository.UpdateAsync(existingAsset);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset with Id {AssetId}", asset.Id);
            throw;
        }

    }

    public async Task ChangeAssetStatusAsync(int id, AssetStatus newStatus)
    {
        try
        {
            var asset = await _assetRepository.GetByIdAsync(id);
            if (asset == null)
                throw new ArgumentException("Not a valid asset Id, asset not found.");

            if (asset.Status == AssetStatus.Assigned && newStatus != AssetStatus.Assigned)
                throw new ArgumentException("Cannot change status of an Assigned asset. Please return it from the Assignments page first.");
                
            if (asset.Status != AssetStatus.Assigned && newStatus == AssetStatus.Assigned)
                throw new ArgumentException("Cannot manually change status to Assigned. Please assign it to an employee via the Assignments page.");

            asset.Status = newStatus;
            await _assetRepository.UpdateAsync(asset);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing status for asset Id {AssetId}", id);
            throw;
        }
    }

    public async Task<AssetSummaryDto> GetAssetSummaryServiceAsync(AssetQueryDto queryDto)
    {
        try
        {
            return await _assetRepository.GetAssetSummaryAsync(queryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset summary");
            throw;
        }
    }

    public async Task<AssetImportValidationResultDto> ValidateImportAsync(List<(int Row, Asset Asset)> assets)
    {
        try
        {
            var serialNumbers = assets.Select(x => x.Asset.SerialNumber.Trim().ToLowerInvariant()).ToHashSet();
            var existingAssets = await _assetRepository.GetExistingSerialNumbersAsync(serialNumbers);
            var errors = new List<ImportRowError>();
            var validAssets = new List<Asset>();

            foreach (var row in assets)
            {
                var serial = row.Asset.SerialNumber.Trim().ToLowerInvariant();
                if (existingAssets.Contains(serial))
                {
                    errors.Add(new ImportRowError(row.Row, $"Serial Number '{row.Asset.SerialNumber}' already exists."));
                    continue;
                }
                validAssets.Add(row.Asset);
            }

            return new AssetImportValidationResultDto(validAssets, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating asset import");
            throw;
        }
    }
}


