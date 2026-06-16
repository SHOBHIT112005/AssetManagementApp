using System.ComponentModel.DataAnnotations;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Domain.Entities;

public class Employee
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter the employee's full name.")]
    [StringLength(100,ErrorMessage = "Employee name must be less than 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a department.")]
    public Department? Department { get; set; }

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(100,ErrorMessage = "Employee email must be less than 100 characters.")]
    public string Email { get; set; } = string.Empty;

    public EmployeeStatus Status { get; set; }
}
// string.Empty avoids nullable warnings