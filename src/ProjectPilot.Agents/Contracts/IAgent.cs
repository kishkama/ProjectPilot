using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectPilot.Agents.Contracts;

/// <summary>
/// Core interface for all ProjectPilot agents
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Unique name identifying this agent
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Type classification of the agent
    /// </summary>
    AgentType AgentType { get; }

    /// <summary>
    /// Human-readable description of agent capabilities
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Process a message and return a response
    /// </summary>
    Task<AgentResponse> ProcessAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a message and stream response chunks
    /// </summary>
    IAsyncEnumerable<AgentResponseChunk> ProcessStreamAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Agent type classification
/// </summary>
public enum AgentType
{
    Orchestrator,
    Planning,
    Research,
    Reporting
}