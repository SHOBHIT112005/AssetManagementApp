using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.Services;


public class AssetAssignmentService : IAssetAssignmentService
{
    private readonly IAssetAssignmentRepository _assetAssignmentRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public AssetAssignmentService(
        IAssetAssignmentRepository assetAssignmentRepository,
        IAssetRepository assetRepository, IEmployeeRepository employeeRepository)
    {
        _assetAssignmentRepository = assetAssignmentRepository;
        _assetRepository = assetRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task AssignAssetAsync(AssetAssignment assetAssignment)
    {
        var assetId = assetAssignment.AssetId;
        var employeeId = assetAssignment.EmployeeId;
        var asset = await _assetRepository.GetByIdAsync(assetId) ?? throw new ArgumentException("Asset not found.");
        var employee = await _employeeRepository.GetByIdAsync(employeeId) ?? throw new ArgumentException("Employee not found.");
        var assignments = await _assetAssignmentRepository.GetByAssetIdAsync(assetId);

        if (assignments.Any(a => a.ReturnDate == null))
        {
            throw new InvalidOperationException("Asset is already assigned.");
        }

        if (asset.Status != AssetStatus.Available)
        {
            throw new ArgumentException("Asset unavailabe, choose a different asset");
        }

        if (employee.Status != EmployeeStatus.Active)
        {
            throw new ArgumentException("Not an active employee, choose an active employee.");
        }

        if (assetAssignment.AssignmentDate == default)
        {
            assetAssignment.AssignmentDate = DateTime.Today;
        }

        asset.Status = AssetStatus.Assigned;
        await _assetRepository.UpdateAsync(asset);
        await _assetAssignmentRepository.AddAsync(assetAssignment);
    }

    public async Task ReturnAssetAsync(int assignmentId)
    {
        var assignment = await _assetAssignmentRepository.GetByIdAsync(assignmentId) ?? throw new ArgumentException("Assignment not found.");

        if (assignment.ReturnDate != null)
        {
            throw new InvalidOperationException("This assignment has already been returned.");
        }

        var asset = await _assetRepository.GetByIdAsync(assignment.AssetId) ?? throw new ArgumentException("Asset not found.");

        asset.Status = AssetStatus.Available;
        assignment.ReturnDate = DateTime.Today;

        await _assetRepository.UpdateAsync(asset);
        await _assetAssignmentRepository.UpdateAsync(assignment);
    }

    public Task<IEnumerable<AssetAssignment>> GetAllAssignmentsAsync()
    {
        return _assetAssignmentRepository.GetAllAsync();
    }

    public async Task<AssetAssignment?> GetAssignmentByIdAsync(int id)
    {
        var assignment = await _assetAssignmentRepository.GetByIdAsync(id) ?? throw new ArgumentException("Assignment not found.");

        return assignment;
    }

    public async Task<IEnumerable<AssetAssignment>> GetAssignmentsByAssetIdAsync(int assetId)
    {
        var asset = await _assetRepository.GetByIdAsync(assetId) ?? throw new ArgumentException("Asset not found.");

        return await _assetAssignmentRepository.GetByAssetIdAsync(assetId);
    }

    public async Task<AssetAssignment?> GetActiveAssignmentByAssetIdAsync(int assetId)
    {
        var asset = await _assetRepository.GetByIdAsync(assetId) ?? throw new ArgumentException("Asset not found.");
        var assignments = await _assetAssignmentRepository.GetByAssetIdAsync(assetId);
        return assignments.FirstOrDefault(a => a.ReturnDate == null) ?? throw new ArgumentException("This asset is currently unassigned.");
    }

    public async Task<IEnumerable<AssetAssignment>> GetAssignmentsByEmployeeIdAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId) ?? throw new ArgumentException("Employee not found.");

        return await _assetAssignmentRepository.GetByEmployeeIdAsync(employeeId);
    }
}