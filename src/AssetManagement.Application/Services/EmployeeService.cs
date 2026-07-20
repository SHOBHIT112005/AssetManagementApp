using AssetManagement.Application.DTOs;
using AssetManagement.Application.DTOs.EmployeeDTOs;
using AssetManagement.Application.Interfaces.Assets;
using AssetManagement.Application.Interfaces.Assignments;
using AssetManagement.Application.Interfaces.Employees;
using AssetManagement.Application.Interfaces.Data;
using AssetManagement.Application.Interfaces.Agent;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums.Assets;
using AssetManagement.Domain.Enums.Assignments;
using AssetManagement.Domain.Enums.Employees;

namespace AssetManagement.Application.Services;

using Microsoft.Extensions.Logging;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly IAssetAssignmentRepository _assetAssignmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IEmployeeRepository employeeRepository, IAssetRepository assetRepository, IAssetAssignmentRepository assetassignmentRepository, IUnitOfWork unitOfWork, ILogger<EmployeeService> logger)
    {
        _employeeRepository = employeeRepository;
        _assetRepository = assetRepository;
        _assetAssignmentRepository = assetassignmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<PagedResultDto<Employee>> GetAllEmployeesAsync(EmployeeQueryDto queryDto)
    {
        if (queryDto.PageNumber < 1)
            queryDto.PageNumber = 1;

        if (queryDto.PageSize < 1)
            queryDto.PageSize = 10;

        return _employeeRepository.GetAllAsync(queryDto);
    }


    public async Task<Employee> GetEmployeeByIdOrThrowAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id) ?? throw new ArgumentException("Employee not found.");
        return employee;
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
    {
        var queryDto = new EmployeeQueryDto
        {
            Status = EmployeeStatus.Active,
            PageSize = int.MaxValue
        };
        var result = await _employeeRepository.GetAllAsync(queryDto);
        return result.Items;
    }

    public async Task CreateEmployeeAsync(Employee employee)
    {
        if (string.IsNullOrWhiteSpace(employee.FullName))
        {
            throw new ArgumentException("Employee name is required.");
        }
        if (string.IsNullOrWhiteSpace(employee.Email))
        {
            throw new ArgumentException("Employee email is required.");
        }
        var phonesToCheck = string.IsNullOrWhiteSpace(employee.PhoneNumber) ? Array.Empty<string>() : new[] { employee.PhoneNumber };
        var existingEmployees = await _employeeRepository.GetExistingEmployeesAsync(new[] { employee.Email }, phonesToCheck);
        if (existingEmployees.Emails.Contains(employee.Email.ToLowerInvariant()))
        {
            throw new ArgumentException("Employee email already exists.");
        }
        if (!string.IsNullOrWhiteSpace(employee.PhoneNumber) && existingEmployees.PhoneNumbers.Contains(employee.PhoneNumber))
        {
            throw new ArgumentException("Employee phone number already exists.");
        }
        employee.Status = EmployeeStatus.Active;
        await _employeeRepository.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CreateBulkEmployeesAsync(IEnumerable<Employee> employees)
    {
        foreach (var employee in employees)
        {
            if (string.IsNullOrWhiteSpace(employee.FullName))
                throw new ArgumentException("Employee name is required.");
            if (string.IsNullOrWhiteSpace(employee.Email))
                throw new ArgumentException("Employee email is required.");
            employee.Status = EmployeeStatus.Active;
            await _employeeRepository.AddAsync(employee);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        var existingEmployee = await _employeeRepository.GetByIdAsync(employee.Id);
        if (existingEmployee == null)
        {
            throw new ArgumentException("Employee not found.");
        }

        if (string.IsNullOrWhiteSpace(employee.FullName))
        {
            throw new ArgumentException("Employee name is required.");
        }
        if (string.IsNullOrWhiteSpace(employee.Email))
        {
            throw new ArgumentException("Employee email is required.");
        }
        var phonesToCheck = string.IsNullOrWhiteSpace(employee.PhoneNumber) ? Array.Empty<string>() : new[] { employee.PhoneNumber };
        var existingEmployees = await _employeeRepository.GetExistingEmployeesAsync(new[] { employee.Email }, phonesToCheck);
        if (existingEmployees.Emails.Contains(employee.Email.ToLowerInvariant()) && !string.Equals(existingEmployee.Email, employee.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Employee email already exists.");
        }
        if (!string.IsNullOrWhiteSpace(employee.PhoneNumber) && 
            existingEmployees.PhoneNumbers.Contains(employee.PhoneNumber) && 
            !string.Equals(existingEmployee.PhoneNumber, employee.PhoneNumber, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Employee phone number already exists.");
        }
        existingEmployee.FullName = employee.FullName;
        existingEmployee.Email = employee.Email;
        existingEmployee.Department = employee.Department;
        existingEmployee.Status = employee.Status;
        existingEmployee.PhoneNumber = employee.PhoneNumber;
        existingEmployee.Designation = employee.Designation;
        await _employeeRepository.UpdateAsync(existingEmployee);
        await _unitOfWork.SaveChangesAsync();
    }
    //To bundle the op in this function to an EF core transaction or use Unit of Work pattern to ensure that either all the operations succeed or none of them do, maintaining data integrity.
    public async Task DeactivateEmployeeAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null)
        {
            throw new ArgumentException("Employee not found.");
        }
        var assignments = await _assetAssignmentRepository.GetByEmployeeIdAsync(id);
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
                await _assetAssignmentRepository.UpdateAsync(assetAssignment);
            }
        }
        employee.Status = EmployeeStatus.Inactive;
        await _employeeRepository.UpdateAsync(employee);
        await _unitOfWork.SaveChangesAsync();
    }
    public async Task ActivateEmployeeAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);

        if (employee == null)
        {
            throw new ArgumentException("Employee not found.");
        }

        employee.Status = EmployeeStatus.Active;

        await _employeeRepository.UpdateAsync(employee);
        await _unitOfWork.SaveChangesAsync();
    }

    public Task<EmployeeSummaryDto> GetEmployeeSummaryServiceAsync(EmployeeQueryDto queryDto)
    {
        return _employeeRepository.GetEmployeeSummaryAsync(queryDto);
    }
    public async Task<EmployeeImportValidationResultDto> ValidateImportAsync(List<(int Row, Employee Employee)> employees)
    {
        var emails = employees.Select(x => x.Employee.Email.Trim().ToLowerInvariant()).ToHashSet();

        var phones = employees.Select(x => x.Employee.PhoneNumber.Trim()).ToHashSet();

        var existing = await _employeeRepository.GetExistingEmployeesAsync(emails, phones);

        var validEmployees = new List<Employee>();
        var errors = new List<ImportRowError>();

        foreach (var row in employees)
        {
            bool invalid = false;

            if (existing.Emails.Contains(row.Employee.Email.Trim().ToLowerInvariant()))
            {
                errors.Add(new ImportRowError(
                    row.Row,
                    $"Email '{row.Employee.Email}' already exists."));

                invalid = true;
            }

            if (existing.PhoneNumbers.Contains(row.Employee.PhoneNumber.Trim()))
            {
                errors.Add(new ImportRowError(
                    row.Row,
                    $"Phone Number '{row.Employee.PhoneNumber}' already exists."));

                invalid = true;
            }

            if (!invalid)
                validEmployees.Add(row.Employee);
        }

        return new EmployeeImportValidationResultDto(validEmployees, errors);
    }
}

