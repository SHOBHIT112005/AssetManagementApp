namespace AssetManagement.Application.DTOs.AssignmentDTOs;

public class AssetAssignmentHistoryDto
{
    public int AssignmentId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string AssetName { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public DateOnly AssignedDate { get; set; }

    public DateOnly? ReturnedDate { get; set; }

    public bool IsReturned { get; set; }
}