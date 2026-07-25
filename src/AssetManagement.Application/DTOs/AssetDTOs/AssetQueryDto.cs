using AssetManagement.Domain.Enums.Assets;

namespace AssetManagement.Application.DTOs.AssetDTOs;

public class AssetQueryDto
{
    public string? SearchTerm { get; set; }
    public AssetStatus? Status { get; set; }
    public AssetType? AssetType { get; set; }
    public AssetSearchField SearchField { get; set; } = AssetSearchField.AssetName;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public AssetSortField? SortField { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
}
