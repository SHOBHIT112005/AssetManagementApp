using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Domain.Entities;

public class AssetAssignment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select the asset to assign.")]
    public int AssetId { get; set; }
    [Required(ErrorMessage = "Please select the employee to assign the asset to.")]
    public int EmployeeId { get; set; }
    [Required(ErrorMessage = "Please enter the assignment date.")]
    public DateTime AssignmentDate { get; set; }
    [Range(typeof(DateTime), "2010-01-01", "2040-12-31", ErrorMessage = "Return date must be between 2010 and 2040.")]
    public DateTime? ReturnDate { get; set; }
    [StringLength(250)]
    public string? Notes { get; set; }
}