using System.Collections.Generic;

namespace ProjectPilot.Agents.Contracts;

/// <summary>
/// Complete response from an agent
/// </summary>
public record AgentResponse
{
    /// <summary>
    /// Unique identifier for this response
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The content of the response
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Name of the agent that generated this response
    /// </summary>
    public required string FromAgent { get; init; }

    /// <summary>
    /// Whether this agent wants to hand off to another agent
    /// </summary>
    public bool RequestsHandoff { get; init; }

    /// <summary>
    /// Target agent for handoff (if RequestsHandoff is true)
    /// </summary>
    public string? HandoffTarget { get; init; }

    /// <summary>
    /// Context to pass to the handoff target
    /// </summary>
    public string? HandoffContext { get; init; }

    /// <summary>
    /// Tool calls made during response generation
    /// </summary>
    public List<ToolCallResult>? ToolCalls { get; init; }

    /// <summary>
    /// Token usage statistics
    /// </summary>
    public TokenUsage? Usage { get; init; }

    /// <summary>
    /// Processing duration in milliseconds
    /// </summary>
    public long DurationMs { get; init; }
}

/// <summary>
/// Streaming response chunk from an agent
/// </summary>
public record AgentResponseChunk
{
    public required string Content { get; init; }
    public required string FromAgent { get; init; }
    public bool IsComplete { get; init; }
    public ChunkType Type { get; init; } = ChunkType.Content;
}

public enum ChunkType
{
    Content,
    ToolCall,
    Thinking,
    Handoff
}