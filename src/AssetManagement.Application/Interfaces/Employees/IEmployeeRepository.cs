using AssetManagement.Application.DTOs;
using AssetManagement.Application.DTOs.EmployeeDTOs;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Interfaces.Employees;

public interface IEmployeeRepository
{
    Task<PagedResultDto<Employee>> GetAllAsync(EmployeeQueryDto queryDto);
    Task<Employee?> GetByIdAsync(int id);
    Task AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task DeleteAsync(int id);
    Task<EmployeeSummaryDto> GetEmployeeSummaryAsync(EmployeeQueryDto queryDto);
    Task<(HashSet<string> Emails, HashSet<string> PhoneNumbers)> GetExistingEmployeesAsync(IEnumerable<string> emails, IEnumerable<string> phoneNumbers);
}
