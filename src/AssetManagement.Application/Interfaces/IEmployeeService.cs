using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Interfaces;


public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();

    Task<Employee?> GetEmployeeByIdAsync(int id);

    Task CreateEmployeeAsync(Employee employee);

    Task UpdateEmployeeAsync(Employee employee);

    Task DeactivateEmployeeAsync(int id);
    
    Task ActivateEmployeeAsync(int id);
}