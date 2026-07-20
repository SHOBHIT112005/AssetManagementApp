using AssetManagement.Domain.Enums.Assets;
using AssetManagement.Domain.Enums.Assignments;
using AssetManagement.Domain.Enums.Employees;

namespace AssetManagement.Application.DTOs.AssignmentDTOs;

public class AssignmentQueryDto
{
    public string? SearchTerm { get; set; }
    public AssignmentSearchField SearchField { get; set; } = AssignmentSearchField.AssetName;
    public bool? ReturnStatus { get; set; }
    public DateOnly? AssignmentDateFrom { get; set; }
    public DateOnly? AssignmentDateTo { get; set; }
    public DateOnly? ReturnDateFrom { get; set; }
    public DateOnly? ReturnDateTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public AssignmentSortField? SortField { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
}

