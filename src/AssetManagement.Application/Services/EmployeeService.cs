using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        return _employeeRepository.GetAllAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id) ?? throw new ArgumentException("Employee not found.");
        return employee;
    }

    public Task CreateEmployeeAsync(Employee employee)
    {
        employee.Status = EmployeeStatus.Active;
        if (string.IsNullOrWhiteSpace(employee.FullName))
        {
            throw new ArgumentException("Employee name is required.");
        }
        if (string.IsNullOrWhiteSpace(employee.Email))
        {
            throw new ArgumentException("Employee email is required.");
        }
        return _employeeRepository.AddAsync(employee);
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        var existingEmployee = await _employeeRepository.GetByIdAsync(employee.Id);
        if (existingEmployee == null)
        {
            throw new ArgumentException("Not a valid employee Id, employee not found.");
        }

        if (string.IsNullOrWhiteSpace(employee.FullName))
        {
            throw new ArgumentException("Employee name is required.");
        }
        if (string.IsNullOrWhiteSpace(employee.Email))
        {
            throw new ArgumentException("Employee email is required.");
        }
        existingEmployee.FullName = employee.FullName;
        existingEmployee.Email = employee.Email;
        existingEmployee.Department = employee.Department;
        existingEmployee.Status = employee.Status;
        await _employeeRepository.UpdateAsync(existingEmployee);
    }

    public async Task DeactivateEmployeeAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);

        if (employee == null)
        {
            throw new ArgumentException("Not a valid employee Id, employee not found.");
        }

        employee.Status = EmployeeStatus.Inactive;

        await _employeeRepository.UpdateAsync(employee);
    }
}