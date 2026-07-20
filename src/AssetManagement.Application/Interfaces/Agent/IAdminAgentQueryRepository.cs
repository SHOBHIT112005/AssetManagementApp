namespace AssetManagement.Application.Interfaces.Agent;

/// <summary>
/// Abstraction for the dynamic database queries used by the Admin AI Agent.
/// Implementation lives in Infrastructure using Dapper.
/// </summary>
public interface IAdminAgentQueryRepository
{
    Task<AgentQueryResult> QueryAssetsAsync(IReadOnlyList<AgentFilterCondition> filters, int limit);
    Task<AgentQueryResult> QueryEmployeesAsync(IReadOnlyList<AgentFilterCondition> filters, int limit);
    Task<AgentQueryResult> QueryAssignmentsAsync(IReadOnlyList<AgentFilterCondition> filters, int limit);

    /// <summary>
    /// Returns the IDs of assets matching the given filters.
    /// Used by the agent write flow to resolve filter-based targets to concrete IDs.
    /// </summary>
    Task<IReadOnlyList<int>> GetAssetIdsByFiltersAsync(IReadOnlyList<AgentFilterCondition> filters, int limit);
}

public sealed record AgentFilterCondition(string Field, string Operator, string Value);

public sealed record AgentQueryResult(
    IReadOnlyList<string> Columns,
    IReadOnlyList<AgentQueryRow> Rows,
    int TotalCount);

public sealed record AgentQueryRow(
    string First,
    string Second,
    string Third,
    string Fourth,
    string Fifth);

