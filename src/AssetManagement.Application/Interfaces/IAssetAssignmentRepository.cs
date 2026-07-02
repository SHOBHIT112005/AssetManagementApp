using AssetManagement.Application.DTOs.AssignmentDTOs;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Interfaces;

public interface IAssetAssignmentRepository
{
    Task <IEnumerable<AssetAssignment>> GetAllAsync();
    Task AddAsync(AssetAssignment assignment);
    Task UpdateAsync(AssetAssignment assignment);
    Task<AssetAssignment?> GetByIdAsync(int id);
    Task<IEnumerable<AssetAssignment>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<AssetAssignment>> GetByAssetIdAsync(int assetId);
    Task<IEnumerable<AssetAssignmentHistoryDto>> GetAssignmentHistoryAsync();
}

