using AssetManagement.Application.DTOs.EmployeeDTOs;

namespace AssetManagement.Application.Interfaces.Employees;

public interface IEmployeeProvisioningService
{
    Task ProvisionEmployeeAsync(EmployeeCreateDto dto);
}
