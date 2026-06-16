using AssetManagement.Domain.Enums;

namespace AssetManagement.Domain.Entities;

public class Asset
{
    public int Id { get; set; }
    public string AssetName { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public AssetStatus Status { get; set; }
}