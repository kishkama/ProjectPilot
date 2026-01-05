using System.Collections.Generic;
using ProjectPilot.LLM;
namespace ProjectPilot.Agents.Contracts;

/// <summary>
/// Context passed to agents during processing
/// </summary>
public record AgentContext
{
    /// <summary>
    /// Unique session identifier
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// User identifier (if authenticated)
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Project identifier (if in project context)
    /// </summary>
    public string? ProjectId { get; init; }

    /// <summary>
    /// Conversation history for this session
    /// </summary>
    public List<AgentMessage> ConversationHistory { get; init; } = new();

    /// <summary>
    /// Short-term memory entries relevant to this conversation
    /// </summary>
    public List<MemoryEntry> ShortTermMemory { get; init; } = new();

    /// <summary>
    /// Long-term memory entries retrieved for context
    /// </summary>
    public List<MemoryEntry> LongTermMemory { get; init; } = new();

    /// <summary>
    /// Current agent state
    /// </summary>
    public AgentState? CurrentState { get; init; }

    /// <summary>
    /// Custom properties for extensibility
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();
}