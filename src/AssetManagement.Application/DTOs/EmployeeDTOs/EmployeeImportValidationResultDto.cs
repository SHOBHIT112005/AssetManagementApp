using AssetManagement.Application.Services;
using AssetManagement.Domain.Entities;
namespace AssetManagement.Application.DTOs.EmployeeDTOs;
public record EmployeeImportValidationResultDto(
    List<Employee> ValidEmployees,
    List<ImportRowError> Errors);