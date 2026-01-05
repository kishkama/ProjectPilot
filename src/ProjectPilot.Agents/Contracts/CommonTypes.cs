using System;
using System.Collections.Generic;

namespace ProjectPilot.Agents.Contracts;

public record TokenUsage
{
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public int TotalTokens { get; init; }
}

// public record MemoryEntry
// {
//     public required string Id { get; init; }
//     public required string SessionId { get; init; }
//     public string? ProjectId { get; init; }
//     public required MemoryType Type { get; init; }
//     public required string Content { get; init; }
//     public DateTimeOffset Timestamp { get; init; }
//     public Dictionary<string, object>? Metadata { get; init; }
// }

// public enum MemoryType
// {
//     Plan,
//     Research,
//     Report,
//     AgentState,
//     Activity
// }

public record AgentState
{
    public required string SessionId { get; init; }
    public required string AgentName { get; init; }
    public Dictionary<string, object> Data { get; init; } = new();
    public DateTimeOffset LastUpdated { get; init; } = DateTimeOffset.UtcNow;
}