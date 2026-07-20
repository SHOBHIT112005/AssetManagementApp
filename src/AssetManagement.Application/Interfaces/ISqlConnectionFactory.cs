using System.Data;

namespace AssetManagement.Application.Interfaces;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
