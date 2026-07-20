using System.Data;
using AssetManagement.Application.Interfaces.Assets;
using AssetManagement.Application.Interfaces.Assignments;
using AssetManagement.Application.Interfaces.Employees;
using AssetManagement.Application.Interfaces.Data;
using AssetManagement.Application.Interfaces.Agent;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using Dapper;

namespace AssetManagement.Infrastructure.Data;

public class SqlDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly date)
    {
        parameter.Value = date.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object value)
    {
        return DateOnly.FromDateTime((DateTime)value);
    }
}

public class SqlDateOnlyNullableTypeHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly? date)
    {
        if (date.HasValue)
            parameter.Value = date.Value.ToDateTime(TimeOnly.MinValue);
        else
            parameter.Value = DBNull.Value;
    }

    public override DateOnly? Parse(object value)
    {
        if (value == null || value is DBNull)
            return null;
        return DateOnly.FromDateTime((DateTime)value);
    }
}

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    static SqlConnectionFactory()
    {
        SqlMapper.AddTypeHandler(new SqlDateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new SqlDateOnlyNullableTypeHandler());
    }

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("Connection string is missing");
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}

