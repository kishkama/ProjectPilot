using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjectPilot.Agents.Contracts;
using ProjectPilot.LLM;

namespace ProjectPilot.Agents.Services;

public interface IAgentStateManager
{
    Task<AgentState?> GetStateAsync(string sessionId, string agentName, CancellationToken ct = default);
    Task SaveStateAsync(AgentState state, CancellationToken ct = default);
    Task ClearStateAsync(string sessionId, CancellationToken ct = default);
}

public class AgentStateManager : IAgentStateManager
{
    private readonly IMemoryStore _memoryStore;
    private readonly ILogger<AgentStateManager> _logger;

    public AgentStateManager(
        IMemoryStore memoryStore,
        ILogger<AgentStateManager> logger)
    {
        _memoryStore = memoryStore;
        _logger = logger;
    }

    public async Task<AgentState?> GetStateAsync(
        string sessionId,
        string agentName,
        CancellationToken ct = default)
    {
        var key = BuildStateKey(sessionId, agentName);
        var entry = await _memoryStore.GetAsync(key, ct);

        if (entry == null) return null;

        return System.Text.Json.JsonSerializer.Deserialize<AgentState>(entry.Content);
    }

    public async Task SaveStateAsync(AgentState state, CancellationToken ct = default)
    {
        var key = BuildStateKey(state.SessionId, state.AgentName);

        var entry = new ProjectPilot.LLM.MemoryEntry
        {
            Id = key,
            SessionId = state.SessionId,
            Type = ProjectPilot.LLM.MemoryType.AgentState,
            Content = System.Text.Json.JsonSerializer.Serialize(state),
            Timestamp = DateTimeOffset.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["agent"] = state.AgentName
            }
        };

        await _memoryStore.StoreAsync(entry, ct);

        _logger.LogDebug(
            "Saved state for agent {Agent} in session {Session}",
            state.AgentName, state.SessionId);
    }

    public async Task ClearStateAsync(string sessionId, CancellationToken ct = default)
    {
        await _memoryStore.DeleteBySessionAsync(sessionId, ProjectPilot.LLM.MemoryType.AgentState, ct);

        _logger.LogInformation("Cleared agent states for session {Session}", sessionId);
    }

    private string BuildStateKey(string sessionId, string agentName)
    {
        return $"state:{sessionId}:{agentName}";
    }
}