using AssetManagement.Application.DTOs;
using AssetManagement.Application.DTOs.EmployeeDTOs;
using AssetManagement.Application.Interfaces.Employees;
using AssetManagement.Application.Interfaces.Data;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums.Employees;
using AssetManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace AssetManagement.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AssetDbContext _context;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public EmployeeRepository(AssetDbContext context, ISqlConnectionFactory sqlConnectionFactory)
    {
        _context = context;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task AddAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
    }

    public async Task<Employee?> GetByIdentityIdAsync(string identityUserId)
    {
        return await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdentityUserId == identityUserId);
    }

    public async Task DeleteAsync(int id)
    {
        var employee = await GetByIdAsync(id);

        if (employee is null)
            return;

        _context.Employees.Remove(employee);
    }

    public async Task<PagedResultDto<Employee>> GetAllAsync(EmployeeQueryDto queryDto)
    {
        var query = _context.Employees.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryDto.SearchTerm))
        {
            switch (queryDto.SearchField)
            {
                case EmployeeSearchField.FullName:
                    query = query.Where(e => e.FullName.Contains(queryDto.SearchTerm));
                    break;
                case EmployeeSearchField.Email:
                    query = query.Where(e => e.Email.Contains(queryDto.SearchTerm));
                    break;
                case EmployeeSearchField.PhoneNumber:
                    query = query.Where(e => e.PhoneNumber.Contains(queryDto.SearchTerm));
                    break;
            }
        }

        if (queryDto.Status.HasValue)
        {
            query = query.Where(e => e.Status == queryDto.Status.Value);
        }

        if (queryDto.Department.HasValue)
        {
            query = query.Where(e => e.Department == queryDto.Department.Value);
        }

        if (queryDto.Designation.HasValue)
        {
            query = query.Where(e => e.Designation == queryDto.Designation.Value);
        }

        if (queryDto.SortField.HasValue)
        {
            switch (queryDto.SortField.Value)
            {
                case EmployeeSortField.FullName:
                    query = queryDto.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(e => e.FullName)
                        : query.OrderByDescending(e => e.FullName);
                    break;
                case EmployeeSortField.Email:
                    query = queryDto.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(e => e.Email)
                        : query.OrderByDescending(e => e.Email);
                    break;
                case EmployeeSortField.Department:
                    query = queryDto.SortDirection == SortDirection.Ascending
                        ? query.OrderBy(e => e.Department)
                        : query.OrderByDescending(e => e.Department);
                    break;
            }
        }
        else
        {
            query = query.OrderBy(e => e.FullName);
        }

        var totalCount = await query.CountAsync();
        var employees = await query
            .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ToListAsync();

        return new PagedResultDto<Employee>
        {
            Items = employees,
            TotalCount = totalCount,
            PageNumber = queryDto.PageNumber,
            PageSize = queryDto.PageSize
        };
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees.FindAsync(id);
    }

    public Task UpdateAsync(Employee employee)
    {
        _context.Employees.Update(employee);
        return Task.CompletedTask;
    }

    public async Task<EmployeeSummaryDto> GetEmployeeSummaryAsync(EmployeeQueryDto queryDto)
    {
        var sql = @"
        SELECT
            COUNT(*) AS TotalEmployees,
            COALESCE(SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END),0) AS ActiveEmployees,
            COALESCE(SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END),0) AS InactiveEmployees
        FROM Employees
        WHERE (@Status IS NULL OR Status = @Status)
        AND (@Department IS NULL OR Department = @Department)
        AND (@Designation IS NULL OR Designation = @Designation);";

        using var connection = _sqlConnectionFactory.CreateConnection();

        return await connection.QuerySingleAsync<EmployeeSummaryDto>(
            sql,
            new
            {
                queryDto.Status,
                queryDto.Department,
                queryDto.Designation
            });
    }

    public async Task<(HashSet<string> Emails, HashSet<string> PhoneNumbers)> GetExistingEmployeesAsync(IEnumerable<string> emails, IEnumerable<string> phoneNumbers)
    {
        var emailSet = emails
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim().ToLowerInvariant())
            .ToHashSet();

        var phoneSet = phoneNumbers
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .ToHashSet();

        var existingEmails = await _context.Employees
            .AsNoTracking()
            .Where(e => emailSet.Contains(e.Email.ToLower()))
            .Select(e => e.Email.ToLower())
            .ToHashSetAsync();

        var existingPhones = await _context.Employees
            .AsNoTracking()
            .Where(e => phoneSet.Contains(e.PhoneNumber))
            .Select(e => e.PhoneNumber)
            .ToHashSetAsync();

        return (existingEmails, existingPhones);
    }
}
