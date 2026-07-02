using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    private readonly IAssetRepository _assetRepository;
    private readonly IAssetAssignmentRepository _assetassignmentRepository;

    public EmployeeService(IEmployeeRepository employeeRepository, IAssetRepository assetRepository, IAssetAssignmentRepository assetassignmentRepository)
    {
        _employeeRepository = employeeRepository;
        _assetRepository = assetRepository;
        _assetassignmentRepository = assetassignmentRepository;
    }

    public Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        return _employeeRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();
        return employees.Where(e => e.Status == EmployeeStatus.Active);
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id) ?? throw new ArgumentException("Employee not found.");
        return employee;
    }

    public Task CreateEmployeeAsync(Employee employee)
    {
        if (string.IsNullOrWhiteSpace(employee.FullName))
        {
            throw new ArgumentException("Employee name is required.");
        }
        if (string.IsNullOrWhiteSpace(employee.Email))
        {
            throw new ArgumentException("Employee email is required.");
        }
        employee.Status = EmployeeStatus.Active;
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
        var assignments = await _assetassignmentRepository.GetByEmployeeIdAsync(id);
        if (assignments.Any())
        {
            foreach (var assetAssignment in assignments.Where(a => a.ReturnDate == null))
            {
                var asset = await _assetRepository.GetByIdAsync(assetAssignment.AssetId);

                if (asset is not null)
                {
                    asset.Status = AssetStatus.Available;
                    await _assetRepository.UpdateAsync(asset);
                }

                assetAssignment.ReturnDate = DateOnly.FromDateTime(DateTime.Today);
                await _assetassignmentRepository.UpdateAsync(assetAssignment);
            }
        }
        employee.Status = EmployeeStatus.Inactive;
        await _employeeRepository.UpdateAsync(employee);
    }
    public async Task ActivateEmployeeAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);

        if (employee == null)
        {
            throw new ArgumentException("Not a valid employee Id, employee not found.");
        }

        employee.Status = EmployeeStatus.Active;

        await _employeeRepository.UpdateAsync(employee);
    }
}