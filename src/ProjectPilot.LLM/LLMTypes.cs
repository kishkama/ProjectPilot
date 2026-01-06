using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
namespace ProjectPilot.LLM;

public record ChatMessage
{
    public required string Role { get; init; }
    public required string Content { get; init; }
}

public record ChatRequest
{
    public required List<ChatMessage> Messages { get; init; }
    public float Temperature { get; init; } = 0.7f;
    public int MaxTokens { get; init; } = 2000;
}

public record ChatResponse
{
    public required string Content { get; init; }
    public UsageDetails? Usage { get; init; }
    public bool IsComplete { get; init; }
}

public record UsageDetails
{
    public int? PromptTokens { get; init; }
    public int? CompletionTokens { get; init; }
    public int? TotalTokens { get; init; }
    
    // Aliases for backward compatibility
    public int? InputTokenCount => PromptTokens;
    public int? OutputTokenCount => CompletionTokens;
    public int? TotalTokenCount => TotalTokens;
}

public interface ILLMProvider
{
    Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ChatResponse> ChatStreamAsync(ChatRequest request, CancellationToken cancellationToken = default);
}

public interface IMemoryStore
{
    Task StoreAsync(MemoryEntry entry, CancellationToken cancellationToken = default);
    Task<MemoryEntry?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task<List<MemoryEntry>> SearchAsync(string query, string? projectId = null, int limit = 10, CancellationToken cancellationToken = default);
    Task<List<MemoryEntry>> GetBySessionAsync(string sessionId, MemoryType type, CancellationToken cancellationToken = default);
    Task DeleteBySessionAsync(string sessionId, MemoryType type, CancellationToken cancellationToken = default);
}

public record MemoryEntry
{
    public required string Id { get; init; }
    public required string SessionId { get; init; }
    public string? ProjectId { get; init; }
    public required MemoryType Type { get; init; }
    public required string Content { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

public enum MemoryType
{
    Plan,
    Research,
    Report,
    AgentState,
    Activity
}

public class InMemoryMemoryStore : IMemoryStore
{
    private readonly Dictionary<string, MemoryEntry> _store = new();
    private readonly Dictionary<string, List<string>> _sessionIndex = new();

    public Task StoreAsync(MemoryEntry entry, CancellationToken cancellationToken = default)
    {
        _store[entry.Id] = entry;
        
        var sessionKey = $"{entry.SessionId}:{entry.Type}";
        if (!_sessionIndex.ContainsKey(sessionKey))
        {
            _sessionIndex[sessionKey] = new List<string>();
        }
        if (!_sessionIndex[sessionKey].Contains(entry.Id))
        {
            _sessionIndex[sessionKey].Add(entry.Id);
        }
        
        return Task.CompletedTask;
    }

    public Task<MemoryEntry?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(key, out var entry);
        return Task.FromResult(entry);
    }

    public Task<List<MemoryEntry>> SearchAsync(string query, string? projectId = null, int limit = 10, CancellationToken cancellationToken = default)
    {
        var results = _store.Values
            .Where(e => e.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(limit)
            .ToList();
        return Task.FromResult(results);
    }

    public Task<List<MemoryEntry>> GetBySessionAsync(string sessionId, MemoryType type, CancellationToken cancellationToken = default)
    {
        var sessionKey = $"{sessionId}:{type}";
        var entryIds = _sessionIndex.GetValueOrDefault(sessionKey, new List<string>());
        var entries = entryIds
            .Select(id => _store.GetValueOrDefault(id))
            .Where(e => e != null)
            .Cast<MemoryEntry>()
            .ToList();
        return Task.FromResult(entries);
    }

    public Task DeleteBySessionAsync(string sessionId, MemoryType type, CancellationToken cancellationToken = default)
    {
        var sessionKey = $"{sessionId}:{type}";
        if (_sessionIndex.TryGetValue(sessionKey, out var entryIds))
        {
            foreach (var id in entryIds)
            {
                _store.Remove(id);
            }
            _sessionIndex.Remove(sessionKey);
        }
        return Task.CompletedTask;
    }
}

// public enum MemoryType
// {
//     Conversation,  // For chat history
//     Project,       // For project-specific context
//     User,          // For user preferences
//     LongTerm       // For general knowledge storage
// }

// public record MemoryEntry
// {
//     public required string Key { get; init; }
//     public required string Content { get; init; }
//     public string? ProjectId { get; init; }
//     public string? SessionId { get; init; }
//     public MemoryType Type { get; init; }
//     public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
//     public Dictionary<string, object>? Metadata { get; init; }
// }
