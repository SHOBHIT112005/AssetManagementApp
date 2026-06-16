using AssetManagement.Domain.Enums;

namespace AssetManagement.Domain.Entities;

public class Asset
{
    public int Id { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public AssetType Type { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public DateTime WarrantyExpiryDate { get; set; }
    public AssetStatus Status { get; set; }
    public AssetCondition Condition { get; set; }
}