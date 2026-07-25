using AssetManagement.Application.DTOs;
using AssetManagement.Application.DTOs.EmployeeDTOs;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Interfaces.Employees;

public interface IEmployeeService
{
    Task<PagedResultDto<Employee>> GetAllEmployeesAsync(EmployeeQueryDto queryDto);
    Task<Employee> GetEmployeeByIdOrThrowAsync(int id);
    Task<Employee?> GetEmployeeByIdentityIdAsync(string identityUserId);
    Task<IEnumerable<Employee>> GetActiveEmployeesAsync();

    Task CreateEmployeeAsync(Employee employee);

    Task UpdateEmployeeAsync(Employee employee);

    Task DeactivateEmployeeAsync(int id);

    Task ActivateEmployeeAsync(int id);
    Task DeleteEmployeeAsync(int id);

    Task<EmployeeSummaryDto> GetEmployeeSummaryServiceAsync(EmployeeQueryDto queryDto);
}
