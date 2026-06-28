using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Domain.Entities;

public class AssetAssignment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select the asset to assign.")]
    public int AssetId { get; set; }

    public Asset Asset { get; set; } = null!; // Navigation property

    [Range(1, int.MaxValue, ErrorMessage = "Please select the employee to assign the asset to.")]
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!; // Navigation property

    [Required(ErrorMessage = "Please enter the assignment date.")]
    public DateOnly AssignmentDate { get; set; }

    [Range(typeof(DateOnly), "2010-01-01", "2040-12-31", ErrorMessage = "Return date must be between 2010 and 2040.")]
    public DateOnly? ReturnDate { get; set; }

    [StringLength(250)]
    public string? Notes { get; set; }
}