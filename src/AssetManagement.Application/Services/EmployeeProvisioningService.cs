using AssetManagement.Application.DTOs.EmployeeDTOs;
using AssetManagement.Application.Interfaces.Data;
using AssetManagement.Application.Interfaces.Employees;
using AssetManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AssetManagement.Application.Services;

public class EmployeeProvisioningService : IEmployeeProvisioningService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmployeeService _employeeService;
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeProvisioningService(
        UserManager<ApplicationUser> userManager,
        IEmployeeService employeeService,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _employeeService = employeeService;
        _unitOfWork = unitOfWork;
    }

    public async Task ProvisionEmployeeAsync(EmployeeCreateDto dto)
    {
        var employee = new Employee
        {
            FullName = dto.FullName,
            Department = dto.Department,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Designation = dto.Designation,
            DateOfBirth = dto.DateOfBirth
        };

        await _employeeService.CreateEmployeeAsync(employee);

        try
        {
            string baseName = dto.FullName.Replace(" ", "");
            string username = $"{baseName}EPN{employee.Id}";

            var user = new ApplicationUser
            {
                UserName = username,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var generatedPassword = GeneratePassword(dto.FullName);
            var result = await _userManager.CreateAsync(user, generatedPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create user account: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "Employee");

            employee.IdentityUserId = user.Id;
            await _employeeService.UpdateEmployeeAsync(employee);
        }
        catch
        {
            await _employeeService.DeleteEmployeeAsync(employee.Id);
            throw;
        }
    }

    private string GeneratePassword(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "Default@123";

        var names = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = string.Join("", names.Select(n => n[0].ToString().ToUpper()));
        var baseName = fullName.Replace(" ", "");
        
        return $"{baseName}123#{initials}";
    }
}
