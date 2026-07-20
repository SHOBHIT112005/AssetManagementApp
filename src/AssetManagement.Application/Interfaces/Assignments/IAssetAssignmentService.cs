using AssetManagement.Application.DTOs;
using AssetManagement.Application.DTOs.AssignmentDTOs;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Interfaces.Assignments;

public interface IAssetAssignmentService
{
    Task AssignAssetAsync(AssetAssignment assignment);
    Task ReturnAssetAsync(int assignmentId);
    Task<AssetAssignment?> GetAssignmentByIdAsync(int id);
    Task<IEnumerable<AssetAssignment>> GetAssignmentsByAssetIdAsync(int assetId);
    Task<AssetAssignment?> GetActiveAssignmentByAssetIdAsync(int assetId);
    Task<IEnumerable<AssetAssignment>> GetAssignmentsByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<AssetAssignment>> GetAllAssignmentsAsync();
    Task<PagedResultDto<AssetAssignmentHistoryDto>> GetAssetAssignmentHistoryAsync(AssignmentQueryDto queryDto);
}
