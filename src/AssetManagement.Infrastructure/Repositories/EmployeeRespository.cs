using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AssetDbContext _context;

    public EmployeeRepository(AssetDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var employee = await GetByIdAsync(id); // Delete operation is not really a read-only query.

        if (employee is null)
            return;

        _context.Employees.Remove(employee);

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Employees.AsNoTracking().ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees.FindAsync(id);
    }

    public async Task UpdateAsync(Employee employee)
    {
        _context.Employees.Update(employee);

        await _context.SaveChangesAsync();
    }
}
