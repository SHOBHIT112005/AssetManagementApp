namespace AssetManagement.Application.DTOs.EmployeeDTOs;

public record EmployeeSummaryDto
{
    public int TotalEmployees { get; init; }
    public int ActiveEmployees { get; init; }
    public int InactiveEmployees { get; init; }
}