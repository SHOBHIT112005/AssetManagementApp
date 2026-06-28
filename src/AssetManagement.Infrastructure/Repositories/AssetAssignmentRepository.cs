using AssetManagement.Application.DTOs;
using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Repositories;

public class AssetAssignmentRepository : IAssetAssignmentRepository
{
    private readonly AssetDbContext _context;

    public AssetAssignmentRepository(AssetDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AssetAssignment>> GetAllAsync()
    {
        return await _context.AssetAssignments.AsNoTracking().ToListAsync();
    }

    public async Task<AssetAssignment?> GetByIdAsync(int id)
    {
        return await _context.AssetAssignments.FindAsync(id);
    }

    public async Task AddAsync(AssetAssignment assetAssignment)
    {
        await _context.AssetAssignments.AddAsync(assetAssignment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AssetAssignment assetAssignment)
    {
        _context.AssetAssignments.Update(assetAssignment);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AssetAssignment>> GetByAssetIdAsync(int assetId)
    {
        return await _context.AssetAssignments.AsNoTracking().Where(a => a.AssetId == assetId).ToListAsync();
    }

    public async Task<IEnumerable<AssetAssignment>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.AssetAssignments.AsNoTracking().Where(a => a.EmployeeId == employeeId).ToListAsync();
    }

    public async Task<IEnumerable<AssetAssignmentHistoryDto>> GetAssignmentHistoryAsync()
    {
        return await _context.AssetAssignments
        .AsNoTracking()
        .Include(a => a.Asset)
        .Include(a => a.Employee)
        .Select(a => new AssetAssignmentHistoryDto
        {
            AssignmentId = a.Id,
            EmployeeName = a.Employee.FullName,
            AssetName = a.Asset.AssetName,
            SerialNumber = a.Asset.SerialNumber,
            AssignedDate = a.AssignmentDate,
            ReturnedDate = a.ReturnDate,
            IsReturned = a.ReturnDate != null
        })
        .ToListAsync();
    }
}