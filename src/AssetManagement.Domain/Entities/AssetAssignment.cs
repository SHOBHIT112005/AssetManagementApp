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
    public DateTime? ReturnDate { get; set; }
    [StringLength(250)]
    public string? Notes { get; set; }
}