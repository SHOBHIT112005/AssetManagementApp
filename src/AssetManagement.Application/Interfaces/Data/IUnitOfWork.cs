using System.Threading.Tasks;

namespace AssetManagement.Application.Interfaces.Data;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}

