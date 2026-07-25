namespace AssetManagement.Application.DTOs.AssetDTOs;

public record AssetSummaryDto
{
    public int TotalAssets { get; init; }
    public int AvailableAssets { get; init; }
    public int AssignedAssets { get; init; }
    public int UnderRepairAssets { get; init; }
    public int RetiredAssets { get; init; }
    public int ExpiringWarrantyAssets { get; init; }
}