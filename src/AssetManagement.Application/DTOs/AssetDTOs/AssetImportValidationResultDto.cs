using AssetManagement.Application.Services;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.DTOs.AssetDTOs;

public record AssetImportValidationResultDto(
    List<Asset> ValidAssets,
    List<ImportRowError> Errors);