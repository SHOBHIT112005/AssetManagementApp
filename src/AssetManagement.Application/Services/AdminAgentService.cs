using System.Text.Json;
using System.Text.Json.Serialization;
using AssetManagement.Application.Interfaces.Agent;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AssetManagement.Application.Services;

public sealed class AdminAgentService
{
    private readonly IAdminAgentQueryRepository _queryRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AdminAgentService> _logger;
    private readonly string _agentEndpoint;

    public AdminAgentService(
        IAdminAgentQueryRepository queryRepository,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AdminAgentService> logger)
    {
        _queryRepository = queryRepository;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _agentEndpoint = configuration["Agent:Endpoint"] ?? "http://localhost:8000/a2a/admin_agent";
    }

    private sealed record FilterCondition(
        string field,
        [property: JsonPropertyName("operator")] string OperatorStr,
        string value);

    private sealed record AgentAction(
        string target_entity,
        List<FilterCondition> filters,
        int limit,
        string summary);

    /// <summary>
    /// Sends a prompt to the Python agent, parses the response, and executes a read query.
    /// </summary>
    public async Task<AdminAgentResponse> AskAsync(string prompt)
    {
        var normalizedPrompt = prompt.Trim();
        if (string.IsNullOrWhiteSpace(normalizedPrompt))
        {
            return AdminAgentResponse.Help("Ask about asset assignments, assets, or employees.");
        }

        var requestPayload = new
        {
            jsonrpc = "2.0",
            id = Guid.NewGuid().ToString(),
            method = "message/send",
            @params = new
            {
                message = new
                {
                    message_id = Guid.NewGuid().ToString(),
                    role = "user",
                    kind = "message",
                    parts = new[]
                    {
                        new { kind = "text", text = normalizedPrompt }
                    }
                }
            }
        };

        HttpResponseMessage httpResponse;
        try
        {
            var httpClient = _httpClientFactory.CreateClient("AgentClient");
            httpResponse = await httpClient.PostAsJsonAsync(_agentEndpoint, requestPayload);
        }
        catch (Exception ex)
        {
            return AdminAgentResponse.Help($"Error connecting to the AI agent: {ex.Message}. Make sure the Python FastAPI server is running.");
        }

        var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

        if (!httpResponse.IsSuccessStatusCode)
        {
            return AdminAgentResponse.Help($"Error from AI agent: HTTP {httpResponse.StatusCode}\n{jsonResponse}");
        }

        AgentAction? action = null;
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        try
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var errorEl))
            {
                return AdminAgentResponse.Help($"Agent JSON-RPC error: {errorEl.GetRawText()}");
            }

            var textContent = "";

            if (root.TryGetProperty("result", out var resultEl))
            {
                if (resultEl.TryGetProperty("status", out var statusEl) &&
                    statusEl.TryGetProperty("state", out var stateEl) &&
                    stateEl.GetString() == "failed")
                {
                    var errMsg = statusEl.TryGetProperty("error", out var err) ? err.GetString() : "Unknown error";
                    return AdminAgentResponse.Help($"AI Task failed: {errMsg}");
                }

                JsonElement? partsEl = null;

                if (resultEl.TryGetProperty("result", out var innerResult) &&
                    innerResult.TryGetProperty("message", out var msgEl) &&
                    msgEl.TryGetProperty("parts", out var p1))
                {
                    partsEl = p1;
                }
                else if (resultEl.TryGetProperty("parts", out var p2))
                {
                    partsEl = p2;
                }

                if (partsEl == null && resultEl.TryGetProperty("artifacts", out var artifactsEl) && artifactsEl.ValueKind == JsonValueKind.Array && artifactsEl.GetArrayLength() > 0)
                {
                    var firstArtifact = artifactsEl[0];
                    if (firstArtifact.TryGetProperty("parts", out var p3))
                    {
                        partsEl = p3;
                    }
                }

                if (partsEl.HasValue && partsEl.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var part in partsEl.Value.EnumerateArray())
                    {
                        if (part.TryGetProperty("text", out var textEl))
                        {
                            textContent += textEl.GetString();
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(textContent))
            {
                textContent = jsonResponse;
            }
            textContent = textContent.Replace("\\\"", "\"").Replace("\\n", "\n");
            var startIndex = textContent.IndexOf("{");
            var endIndex = textContent.LastIndexOf("}");
            if (startIndex >= 0 && endIndex > startIndex)
            {
                var jsonStr = textContent.Substring(startIndex, endIndex - startIndex + 1);
                action = JsonSerializer.Deserialize<AgentAction>(jsonStr, options);
            }
        }
        catch (Exception ex)
        {
            return AdminAgentResponse.Help($"Failed to parse agent response: {ex.Message}");
        }

        if (action == null || action.target_entity == null)
        {
            if (jsonResponse.Contains("503") || jsonResponse.Contains("UNAVAILABLE") || jsonResponse.Contains("high demand") || jsonResponse.Contains("APIError"))
            {
                return AdminAgentResponse.Help("The AI model is currently experiencing high demand (503). Please try again in a moment.");
            }
            return AdminAgentResponse.Help("I'm not sure how to handle that request. (AI returned an invalid structure or failed during generation)");
        }

        return await HandleQueryAsync(action);
    }

    /// <summary>
    /// Handles read queries.
    /// </summary>
    private async Task<AdminAgentResponse> HandleQueryAsync(AgentAction action)
    {
        var filters = (action.filters ?? new List<FilterCondition>())
            .Select(f => new AgentFilterCondition(f.field, f.OperatorStr, f.value))
            .ToList();

        var limit = action.limit > 0 ? action.limit : 20;

        AgentQueryResult queryResult;
        string[] suggestions;

        if (action.target_entity == "Asset")
        {
            queryResult = await _queryRepository.QueryAssetsAsync(filters, limit);
            suggestions = ["Show all active laptops", "List assets in repair"];
        }
        else if (action.target_entity == "Employee")
        {
            queryResult = await _queryRepository.QueryEmployeesAsync(filters, limit);
            suggestions = ["Show Engineering department employees", "Show all active employees"];
        }
        else
        {
            queryResult = await _queryRepository.QueryAssignmentsAsync(filters, limit);
            suggestions = ["Show all unreturned laptops", "Show assignments for the past month"];
        }

        var summary = queryResult.TotalCount == 0
            ? "No data with these specific filters is available in the database."
            : action.summary;

        var rows = queryResult.Rows.Select(r => new AdminAgentRow(r.First, r.Second, r.Third, r.Fourth, r.Fifth)).ToList();

        return AdminAgentResponse.WithRows(
            summary,
            queryResult.Columns,
            rows,
            suggestions,
            queryResult.TotalCount > limit);
    }
}

public sealed record AdminAgentResponse(
    string Summary,
    IReadOnlyList<string> Columns,
    IReadOnlyList<AdminAgentRow> Rows,
    IReadOnlyList<string> Suggestions,
    bool IsTruncated)
{
    public static AdminAgentResponse Help(string summary) =>
        new(summary, [], [], ["Show laptops assigned recently", "Show active assignments"], false);

    public static AdminAgentResponse WithRows(
        string summary,
        IReadOnlyList<string> columns,
        IReadOnlyList<AdminAgentRow> rows,
        IReadOnlyList<string> suggestions,
        bool isTruncated) =>
        new(summary, columns, rows, suggestions, isTruncated);
}

public sealed record AdminAgentRow(
    string First,
    string Second,
    string Third,
    string Fourth,
    string Fifth);
