using System;

namespace ProjectPilot.Agents.Contracts;

/// <summary>
/// Represents a message sent to or from an agent
/// </summary>
public record AgentMessage
{
    /// <summary>
    /// Unique identifier for this message
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The content of the message
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Role of the message sender
    /// </summary>
    public required MessageRole Role { get; init; }

    /// <summary>
    /// Name of the sending agent (if from an agent)
    /// </summary>
    public string? FromAgent { get; init; }

    /// <summary>
    /// Name of the target agent (if directed to specific agent)
    /// </summary>
    public string? ToAgent { get; init; }

    /// <summary>
    /// Timestamp when message was created
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional metadata attached to the message
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

public enum MessageRole
{
    User,
    Agent,
    System,
    Tool
}