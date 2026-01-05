using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjectPilot.Agents.Contracts;
using ProjectPilot.LLM;

namespace ProjectPilot.Agents.Services;

public interface IAgentActivityLogger
{
    Task LogActivityAsync(AgentActivity activity, CancellationToken ct = default);
    Task LogCommunicationAsync(AgentCommunication communication, CancellationToken ct = default);
    Task LogHandoffAsync(AgentHandoff handoff, CancellationToken ct = default);
    Task<List<AgentActivity>> GetActivitiesAsync(string sessionId, CancellationToken ct = default);
    Task<List<AgentCommunication>> GetCommunicationHistoryAsync(string sessionId, CancellationToken ct = default);
}

public record AgentActivity
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public required string SessionId { get; init; }
    public required string AgentName { get; init; }
    public required AgentActivityType Type { get; init; }
    public required string Description { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public long? DurationMs { get; init; }
}

public enum AgentActivityType
{
    MessageReceived,
    MessageSent,
    ToolInvoked,
    Handoff,
    StateChanged,
    Error
}

public class AgentActivityLogger : IAgentActivityLogger
{
    private readonly IMemoryStore _memoryStore;
    private readonly ILogger<AgentActivityLogger> _logger;

    public AgentActivityLogger(
        IMemoryStore memoryStore,
        ILogger<AgentActivityLogger> logger)
    {
        _memoryStore = memoryStore;
        _logger = logger;
    }

    public async Task LogActivityAsync(AgentActivity activity, CancellationToken ct = default)
    {
        var entry = new ProjectPilot.LLM.MemoryEntry
        {
            Id = activity.Id,
            SessionId = activity.SessionId,
            Type = ProjectPilot.LLM.MemoryType.Activity,
            Content = System.Text.Json.JsonSerializer.Serialize(activity),
            Timestamp = activity.Timestamp,
            Metadata = new Dictionary<string, object>
            {
                ["agent"] = activity.AgentName,
                ["activity_type"] = activity.Type.ToString()
            }
        };

        await _memoryStore.StoreAsync(entry, ct);

        _logger.LogDebug(
            "Agent {Agent} activity: {Type} - {Description}",
            activity.AgentName, activity.Type, activity.Description);
    }

    public async Task LogCommunicationAsync(AgentCommunication communication, CancellationToken ct = default)
    {
        var activity = new AgentActivity
        {
            SessionId = communication.SessionId ?? "unknown",
            AgentName = communication.FromAgent,
            Type = AgentActivityType.MessageSent,
            Description = $"Sent message to {communication.ToAgent}",
            Metadata = new Dictionary<string, object>
            {
                ["to_agent"] = communication.ToAgent,
                ["communication_type"] = communication.Type.ToString()
            },
            Timestamp = communication.Timestamp
        };

        await LogActivityAsync(activity, ct);
    }

    public async Task LogHandoffAsync(AgentHandoff handoff, CancellationToken ct = default)
    {
        var activity = new AgentActivity
        {
            SessionId = handoff.SessionId ?? "unknown",
            AgentName = handoff.FromAgent,
            Type = AgentActivityType.Handoff,
            Description = $"Handed off to {handoff.ToAgent}",
            Metadata = new Dictionary<string, object>
            {
                ["to_agent"] = handoff.ToAgent,
                ["reason"] = handoff.Reason ?? "unspecified"
            },
            Timestamp = handoff.Timestamp
        };

        await LogActivityAsync(activity, ct);
    }

    public async Task<List<AgentActivity>> GetActivitiesAsync(string sessionId, CancellationToken ct = default)
    {
        var entries = await _memoryStore.GetBySessionAsync(sessionId, ProjectPilot.LLM.MemoryType.Activity, ct);

        return entries
            .Select(e => System.Text.Json.JsonSerializer.Deserialize<AgentActivity>(e.Content)!)
            .OrderBy(a => a.Timestamp)
            .ToList();
    }

    public async Task<List<AgentCommunication>> GetCommunicationHistoryAsync(
        string sessionId,
        CancellationToken ct = default)
    {
        var activities = await GetActivitiesAsync(sessionId, ct);

        return activities
            .Where(a => a.Type == AgentActivityType.MessageSent || a.Type == AgentActivityType.Handoff)
            .Select(a => new AgentCommunication
            {
                FromAgent = a.AgentName,
                ToAgent = a.Metadata?["to_agent"]?.ToString() ?? "unknown",
                SessionId = a.SessionId,
                Timestamp = a.Timestamp
            })
            .ToList();
    }
}