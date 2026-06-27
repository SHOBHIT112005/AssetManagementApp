using System.ComponentModel.DataAnnotations;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Domain.Entities;

public class Asset
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Please enter the asset name.")]
    [StringLength(100)]
    public string AssetName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Please select the asset type.")]
    public AssetType Type { get; set; }
    [Required(ErrorMessage = "Please enter the asset's serial number.")]
    [StringLength(10)]
    public string SerialNumber { get; set; } = string.Empty;
    [Required(ErrorMessage = "Please enter the purchase date.")]
    [Range(typeof(DateTime), "2010-01-01", "2040-12-31", ErrorMessage = "Purchase date must be between 2010 and 2040.")]
    public DateTime PurchaseDate { get; set; }
    [Required(ErrorMessage = "Please enter the warranty expiry date.")]
    [Range(typeof(DateTime), "2010-01-01", "2040-12-31", ErrorMessage = "Warranty expiry date must be between 2010 and 2040.")]
    public DateTime WarrantyExpiryDate { get; set; }
    [Required(ErrorMessage = "Please select the asset status.")]
    public AssetStatus Status { get; set; }
    [Required(ErrorMessage = "Please select the asset condition.")]
    public AssetCondition Condition { get; set; }
}