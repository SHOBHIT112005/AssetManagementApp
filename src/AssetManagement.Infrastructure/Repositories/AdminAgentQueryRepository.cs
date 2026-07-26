using System.Globalization;
using AssetManagement.Application.Interfaces.Agent;
using AssetManagement.Application.Interfaces.Data;
using AssetManagement.Domain.Enums.Assets;
using AssetManagement.Domain.Enums.Employees;
using Dapper;

namespace AssetManagement.Infrastructure.Repositories;

public sealed class AdminAgentQueryRepository : IAdminAgentQueryRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public AdminAgentQueryRepository(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<AgentQueryResult> QueryAssetsAsync(IReadOnlyList<AgentFilterCondition> filters, int limit)
    {
        var builder = new SqlBuilder();
        BuildAssetFilters(builder, filters, out var ignoredFilters);

        var dataTemplate = builder.AddTemplate(@"
            SELECT TOP (@Limit)
                a.AssetName, a.Type, a.SerialNumber, a.PurchaseDate, a.WarrantyExpiryDate, a.Status, a.Condition
            FROM Assets a
            /**where**/
            ORDER BY a.AssetName
        ", new { Limit = limit });

        var countTemplate = builder.AddTemplate(@"
            SELECT COUNT(*) FROM Assets a /**where**/
        ");

        using var connection = _sqlConnectionFactory.CreateConnection();
        var results = (await connection.QueryAsync<AssetRow>(dataTemplate.RawSql, dataTemplate.Parameters)).ToList();
        var totalCount = await connection.ExecuteScalarAsync<int>(countTemplate.RawSql, countTemplate.Parameters);

        var rows = results.Select(a => new AgentQueryRow([
            a.AssetName,
            ((AssetType)a.Type).ToString(),
            a.SerialNumber,
            FormatDate(a.PurchaseDate),
            FormatDate(a.WarrantyExpiryDate),
            ((AssetStatus)a.Status).ToString(),
            ((AssetCondition)a.Condition).ToString()
        ])).ToList();

        return new AgentQueryResult(
            ["Asset Name", "Type", "Serial No.", "Purchase Date", "Warranty Expiry", "Status", "Condition"],
            rows,
            totalCount);
    }

    public async Task<AgentQueryResult> QueryEmployeesAsync(IReadOnlyList<AgentFilterCondition> filters, int limit)
    {
        var builder = new SqlBuilder();
        var ignoredFilters = new List<string>();

        foreach (var filter in filters)
        {
            var op = filter.Operator;
            var val = filter.Value ?? "";

            if (filter.Field == "Department")
            {
                ApplyEnumFilter<Department>(builder, "e.Department", op, val, ignoredFilters, filter.Field);
            }
            else if (filter.Field == "Designation")
            {
                ApplyEnumFilter<EmployeeDesignation>(builder, "e.Designation", op, val, ignoredFilters, filter.Field);
            }
            else if (filter.Field == "Status")
            {
                ApplyEnumFilter<EmployeeStatus>(builder, "e.Status", op, val, ignoredFilters, filter.Field);
            }
            else if (filter.Field == "FullName")
            {
                if (op == "contains") builder.Where("e.FullName LIKE @NameVal", new { NameVal = $"%{val}%" });
                else if (op == "==") builder.Where("e.FullName = @NameVal", new { NameVal = val });
            }
            else if (filter.Field == "Email")
            {
                if (op == "contains") builder.Where("e.Email LIKE @EmailVal", new { EmailVal = $"%{val}%" });
                else if (op == "==") builder.Where("e.Email = @EmailVal", new { EmailVal = val });
            }
            else if (filter.Field == "DateOfBirth")
            {
                ApplyDateFilter(builder, "e.DateOfBirth", "dob", op, val);
            }
            else
            {
                ignoredFilters.Add($"'{filter.Field} {op} {val}'");
            }
        }

        var dataTemplate = builder.AddTemplate(@"
            SELECT TOP (@Limit)
                e.FullName, e.Department, e.Designation, e.Email, e.DateOfBirth, e.Status
            FROM Employees e
            /**where**/
            ORDER BY e.FullName
        ", new { Limit = limit });

        var countTemplate = builder.AddTemplate(@"
            SELECT COUNT(*) FROM Employees e /**where**/
        ");

        using var connection = _sqlConnectionFactory.CreateConnection();
        var results = (await connection.QueryAsync<EmployeeRow>(dataTemplate.RawSql, dataTemplate.Parameters)).ToList();
        var totalCount = await connection.ExecuteScalarAsync<int>(countTemplate.RawSql, countTemplate.Parameters);

        var rows = results.Select(e => new AgentQueryRow([
            e.FullName,
            ((Department)e.Department).ToString(),
            ((EmployeeDesignation)e.Designation).ToString(),
            e.Email,
            e.DateOfBirth.HasValue ? FormatDate(e.DateOfBirth.Value) : "N/A",
            ((EmployeeStatus)e.Status).ToString()
        ])).ToList();

        return new AgentQueryResult(
            ["Name", "Department", "Designation", "Email", "Date Of Birth", "Status"],
            rows,
            totalCount);
    }

    public async Task<AgentQueryResult> QueryAssignmentsAsync(IReadOnlyList<AgentFilterCondition> filters, int limit)
    {
        var builder = new SqlBuilder();
        var ignoredFilters = new List<string>();

        foreach (var filter in filters)
        {
            var op = filter.Operator;
            var val = filter.Value ?? "";

            if (filter.Field == "ReturnDate")
            {
                if (op == "is_null") builder.Where("aa.ReturnDate IS NULL");
                else if (op == "is_not_null") builder.Where("aa.ReturnDate IS NOT NULL");
                else ApplyDateFilter(builder, "aa.ReturnDate", "ReturnDateVal", op, val);
            }
            else if (filter.Field == "AssignmentDate")
            {
                ApplyDateFilter(builder, "aa.AssignmentDate", "AssignDateVal", op, val);
            }
            else if (filter.Field == "Employee.FullName")
            {
                if (op == "contains") builder.Where("e.FullName LIKE @EmpNameVal", new { EmpNameVal = $"%{val}%" });
                else if (op == "==") builder.Where("e.FullName = @EmpNameVal", new { EmpNameVal = val });
            }
            else if (filter.Field == "Asset.Type")
            {
                ApplyEnumFilter<AssetType>(builder, "a.Type", op, val, ignoredFilters, filter.Field);
            }
            else if (filter.Field == "Asset.AssetName")
            {
                if (op == "contains") builder.Where("a.AssetName LIKE @AssetNameVal", new { AssetNameVal = $"%{val}%" });
                else if (op == "==") builder.Where("a.AssetName = @AssetNameVal", new { AssetNameVal = val });
            }
            else
            {
                ignoredFilters.Add($"'{filter.Field} {op} {val}'");
            }
        }

        var dataTemplate = builder.AddTemplate(@"
            SELECT TOP (@Limit)
                e.FullName AS EmployeeName,
                a.AssetName,
                a.Type AS AssetType,
                aa.AssignmentDate,
                aa.ReturnDate
            FROM AssetAssignments aa
            INNER JOIN Assets a ON aa.AssetId = a.Id
            INNER JOIN Employees e ON aa.EmployeeId = e.Id
            /**where**/
            ORDER BY aa.AssignmentDate DESC
        ", new { Limit = limit });

        var countTemplate = builder.AddTemplate(@"
            SELECT COUNT(*)
            FROM AssetAssignments aa
            INNER JOIN Assets a ON aa.AssetId = a.Id
            INNER JOIN Employees e ON aa.EmployeeId = e.Id
            /**where**/
        ");

        using var connection = _sqlConnectionFactory.CreateConnection();
        var results = (await connection.QueryAsync<AssignmentRow>(dataTemplate.RawSql, dataTemplate.Parameters)).ToList();
        var totalCount = await connection.ExecuteScalarAsync<int>(countTemplate.RawSql, countTemplate.Parameters);

        var rows = results.Select(a => new AgentQueryRow([
            a.EmployeeName,
            a.AssetName,
            ((AssetType)a.AssetType).ToString(),
            FormatDate(a.AssignmentDate),
            FormatStatus(a.ReturnDate)
        ])).ToList();

        return new AgentQueryResult(
            ["Employee", "Asset", "Type", "Assigned On", "Status"],
            rows,
            totalCount);
    }

    private static void BuildAssetFilters(SqlBuilder builder, IReadOnlyList<AgentFilterCondition> filters, out List<string> ignoredFilters)
    {
        ignoredFilters = new List<string>();

        foreach (var filter in filters)
        {
            var op = filter.Operator;
            var val = filter.Value ?? "";

            if (filter.Field == "Type")
            {
                ApplyEnumFilter<AssetType>(builder, "a.Type", op, val, ignoredFilters, filter.Field);
            }
            else if (filter.Field == "Status")
            {
                ApplyEnumFilter<AssetStatus>(builder, "a.Status", op, val, ignoredFilters, filter.Field);
            }
            else if (filter.Field == "Condition")
            {
                ApplyEnumFilter<AssetCondition>(builder, "a.Condition", op, val, ignoredFilters, filter.Field);
            }
            else if (filter.Field == "AssetName")
            {
                if (op == "contains") builder.Where("a.AssetName LIKE @NameVal", new { NameVal = $"%{val}%" });
                else if (op == "==") builder.Where("a.AssetName = @NameVal", new { NameVal = val });
            }
            else if (filter.Field == "SerialNumber")
            {
                if (op == "==") builder.Where("a.SerialNumber = @SerialVal", new { SerialVal = val });
            }
            else if (filter.Field == "PurchaseDate")
            {
                ApplyDateFilter(builder, "a.PurchaseDate", "PurchaseDateVal", op, val);
            }
            else if (filter.Field == "WarrantyExpiryDate")
            {
                ApplyDateFilter(builder, "a.WarrantyExpiryDate", "WarrantyDateVal", op, val);
            }
            else
            {
                ignoredFilters.Add($"'{filter.Field} {op} {val}'");
            }
        }
    }

    public async Task<IReadOnlyList<int>> GetAssetIdsByFiltersAsync(IReadOnlyList<AgentFilterCondition> filters, int limit)
    {
        var builder = new SqlBuilder();
        BuildAssetFilters(builder, filters, out _);

        var template = builder.AddTemplate(@"
            SELECT TOP (@Limit) a.Id
            FROM Assets a
            /**where**/
            ORDER BY a.Id
        ", new { Limit = limit });

        using var connection = _sqlConnectionFactory.CreateConnection();
        var ids = (await connection.QueryAsync<int>(template.RawSql, template.Parameters)).ToList();
        return ids;
    }

    private static void ApplyEnumFilter<TEnum>(SqlBuilder builder, string column, string op, string val, List<string> ignoredFilters, string filterField) where TEnum : struct, Enum
    {
        if (op == "==" && Enum.TryParse<TEnum>(val, true, out var exactVal))
        {
            var paramName = $"{column.Replace(".", "")}_{Guid.NewGuid():N}";
            var parameters = new DynamicParameters();
            parameters.Add(paramName, (int)(object)exactVal);
            builder.Where($"{column} = @{paramName}", parameters);
        }
        else if (op == "!=" && Enum.TryParse<TEnum>(val, true, out var notExactVal))
        {
            var paramName = $"{column.Replace(".", "")}_{Guid.NewGuid():N}";
            var parameters = new DynamicParameters();
            parameters.Add(paramName, (int)(object)notExactVal);
            builder.Where($"{column} <> @{paramName}", parameters);
        }
        else if (op == "contains")
        {
            var matchingValues = Enum.GetValues<TEnum>()
                .Where(e => e.ToString().Contains(val, StringComparison.OrdinalIgnoreCase))
                .Select(e => (int)(object)e)
                .ToList();

            if (matchingValues.Any())
            {
                var paramName = $"{column.Replace(".", "")}_{Guid.NewGuid():N}";
                var parameters = new DynamicParameters();
                parameters.Add(paramName, matchingValues);
                builder.Where($"{column} IN @{paramName}", parameters);
            }
            else
            {
                builder.Where("1 = 0");
            }
        }
        else
        {
            ignoredFilters.Add($"'{filterField} {op} {val}'");
        }
    }

    private static void ApplyDateFilter(SqlBuilder builder, string column, string baseParamName, string op, string val)
    {
        if (!DateOnly.TryParse(val, CultureInfo.InvariantCulture, out var dateVal))
            return;

        var paramName = $"{baseParamName}_{Guid.NewGuid():N}";
        var dateParam = dateVal.ToDateTime(TimeOnly.MinValue);
        var parameters = new DynamicParameters();
        parameters.Add(paramName, dateParam);

        if (op == ">") builder.Where($"{column} > @{paramName}", parameters);
        else if (op == "<") builder.Where($"{column} < @{paramName}", parameters);
        else if (op == "==") builder.Where($"{column} = @{paramName}", parameters);
        else if (op == ">=") builder.Where($"{column} >= @{paramName}", parameters);
        else if (op == "<=") builder.Where($"{column} <= @{paramName}", parameters);
    }

    private static string FormatDate(DateOnly date) =>
        date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

    private static string FormatStatus(DateOnly? returnDate) =>
        returnDate == null ? "Assigned" : $"Returned {FormatDate(returnDate.Value)}";

    private sealed class AssetRow
    {
        public string AssetName { get; set; } = string.Empty;
        public int Type { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public DateOnly PurchaseDate { get; set; }
        public DateOnly WarrantyExpiryDate { get; set; }
        public int Status { get; set; }
        public int Condition { get; set; }
    }

    private sealed class EmployeeRow
    {
        public string FullName { get; set; } = string.Empty;
        public int Department { get; set; }
        public int Designation { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public int Status { get; set; }
    }

    private sealed class AssignmentRow
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public int AssetType { get; set; }
        public DateOnly AssignmentDate { get; set; }
        public DateOnly? ReturnDate { get; set; }
    }
}
