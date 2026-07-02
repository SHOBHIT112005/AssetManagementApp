using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.DTOs;

public class AssetQueryDto
{
    public string? SearchTerm { get; set; }

    public AssetStatus? Status { get; set; }

    public AssetType? AssetType { get; set; }

    public AssetSearchField SearchField { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}