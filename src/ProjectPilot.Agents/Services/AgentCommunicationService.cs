using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjectPilot.Agents.Contracts;

namespace ProjectPilot.Agents.Services;

public interface IAgentCommunicationService
{
    /// <summary>
    /// Send a message from one agent to another
    /// </summary>
    Task<AgentResponse> SendMessageAsync(
        string fromAgent,
        string toAgent,
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcast a message to all agents
    /// </summary>
    Task BroadcastAsync(
        string fromAgent,
        AgentMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log an agent handoff
    /// </summary>
    Task LogHandoffAsync(
        string fromAgent,
        string toAgent,
        AgentContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get communication history for a session
    /// </summary>
    Task<List<AgentCommunication>> GetHistoryAsync(
        string sessionId,
        CancellationToken cancellationToken = default);
}

public class AgentCommunicationService : IAgentCommunicationService
{
    private readonly IAgentRegistry _agentRegistry;
    private readonly IAgentActivityLogger _activityLogger;
    private readonly ILogger<AgentCommunicationService> _logger;

    public AgentCommunicationService(
        IAgentRegistry agentRegistry,
        IAgentActivityLogger activityLogger,
        ILogger<AgentCommunicationService> logger)
    {
        _agentRegistry = agentRegistry;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<AgentResponse> SendMessageAsync(
        string fromAgent,
        string toAgent,
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Agent {FromAgent} sending message to {ToAgent}",
            fromAgent, toAgent);

        var targetAgent = _agentRegistry.GetAgentByName(toAgent);
        if (targetAgent == null)
        {
            throw new InvalidOperationException($"Agent '{toAgent}' not found");
        }

        // Log the communication
        await _activityLogger.LogCommunicationAsync(new AgentCommunication
        {
            FromAgent = fromAgent,
            ToAgent = toAgent,
            Message = message,
            SessionId = context.SessionId,
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken);

        // Process with target agent
        return await targetAgent.ProcessAsync(message, context, cancellationToken);
    }

    public async Task BroadcastAsync(
        string fromAgent,
        AgentMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Agent {FromAgent} broadcasting message to all agents",
            fromAgent);

        var agents = _agentRegistry.GetAllAgents()
            .Where(a => a.Name != fromAgent);

        foreach (var agent in agents)
        {
            await _activityLogger.LogCommunicationAsync(new AgentCommunication
            {
                FromAgent = fromAgent,
                ToAgent = agent.Name,
                Message = message,
                Type = CommunicationType.Broadcast,
                Timestamp = DateTimeOffset.UtcNow
            }, cancellationToken);
        }
    }

    public async Task LogHandoffAsync(
        string fromAgent,
        string toAgent,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        await _activityLogger.LogHandoffAsync(new AgentHandoff
        {
            FromAgent = fromAgent,
            ToAgent = toAgent,
            SessionId = context.SessionId,
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken);
    }

    public Task<List<AgentCommunication>> GetHistoryAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        return _activityLogger.GetCommunicationHistoryAsync(sessionId, cancellationToken);
    }
}

public record AgentCommunication
{
    public string FromAgent { get; init; } = string.Empty;
    public string ToAgent { get; init; } = string.Empty;
    public AgentMessage? Message { get; init; }
    public string? SessionId { get; init; }
    public CommunicationType Type { get; init; } = CommunicationType.Direct;
    public DateTimeOffset Timestamp { get; init; }
}

public enum CommunicationType
{
    Direct,
    Broadcast,
    Handoff
}

public record AgentHandoff
{
    public string FromAgent { get; init; } = string.Empty;
    public string ToAgent { get; init; } = string.Empty;
    public string? SessionId { get; init; }
    public string? Reason { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}