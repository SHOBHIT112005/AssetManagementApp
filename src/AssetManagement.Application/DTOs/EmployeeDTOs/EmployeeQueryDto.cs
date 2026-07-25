using AssetManagement.Domain.Enums.Employees;

namespace AssetManagement.Application.DTOs.EmployeeDTOs;

public class EmployeeQueryDto
{
    public string? SearchTerm { get; set; }
    public EmployeeStatus? Status { get; set; }
    public Department? Department { get; set; }
    public EmployeeDesignation? Designation { get; set; }
    public EmployeeSearchField SearchField { get; set; } = EmployeeSearchField.FullName;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public EmployeeSortField? SortField { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
}

