namespace AssetManagement.Application.Interfaces.Agent;

public interface IAdminAgentQueryRepository
{
    Task<AgentQueryResult> QueryAssetsAsync(IReadOnlyList<AgentFilterCondition> filters, int limit);
    Task<AgentQueryResult> QueryEmployeesAsync(IReadOnlyList<AgentFilterCondition> filters, int limit);
    Task<AgentQueryResult> QueryAssignmentsAsync(IReadOnlyList<AgentFilterCondition> filters, int limit);
    Task<IReadOnlyList<int>> GetAssetIdsByFiltersAsync(IReadOnlyList<AgentFilterCondition> filters, int limit);
}

public sealed record AgentFilterCondition(string Field, string Operator, string Value);

public sealed record AgentQueryResult(
    IReadOnlyList<string> Columns,
    IReadOnlyList<AgentQueryRow> Rows,
    int TotalCount);

public sealed record AgentQueryRow(IReadOnlyList<string> Values);

