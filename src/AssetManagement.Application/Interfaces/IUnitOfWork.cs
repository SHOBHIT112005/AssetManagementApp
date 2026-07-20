namespace AssetManagement.Application.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}
