using System.Data;

namespace AssetManagement.Application.Interfaces.Data;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}

