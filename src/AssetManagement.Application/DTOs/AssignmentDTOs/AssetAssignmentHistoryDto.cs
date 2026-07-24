namespace AssetManagement.Application.DTOs.AssignmentDTOs;

public record AssetAssignmentHistoryDto
{
    public int AssignmentId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string AssetName { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public DateOnly AssignedDate { get; set; }

    public DateOnly? ReturnedDate { get; set; }

    public bool IsReturned { get; set; }


    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public AssetManagement.Domain.Enums.Employees.Department EmployeeDepartment { get; set; }
    public AssetManagement.Domain.Enums.Employees.EmployeeDesignation EmployeeDesignation { get; set; }
    public AssetManagement.Domain.Enums.Assets.AssetType AssetType { get; set; }
    public AssetManagement.Domain.Enums.Assets.AssetCondition AssetCondition { get; set; }
}