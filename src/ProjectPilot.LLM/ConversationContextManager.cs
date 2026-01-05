using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ProjectPilot.LLM;

public interface IConversationContextManager
{
    Task<ConversationContext> LoadContextAsync(string sessionId, CancellationToken cancellationToken = default);
    Task SaveContextAsync(ConversationContext context, CancellationToken cancellationToken = default);
    Task AddMessageAsync(string sessionId, ChatMessage message, CancellationToken cancellationToken = default);
    Task<List<ChatMessage>> GetRecentMessagesAsync(string sessionId, int limit = 10, CancellationToken cancellationToken = default);
}

public class ConversationContextManager : IConversationContextManager
{
    private readonly IMemoryStore _shortTermMemory;
    private readonly IMemoryStore _longTermMemory;
    private readonly ILogger<ConversationContextManager> _logger;

    public ConversationContextManager(
        IMemoryStoreFactory memoryFactory,
        ILogger<ConversationContextManager> logger)
    {
        _shortTermMemory = memoryFactory.CreateShortTermStore();
        _longTermMemory = memoryFactory.CreateLongTermStore();
        _logger = logger;
    }

    public async Task<ConversationContext> LoadContextAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Load recent conversation messages from short-term memory
            var conversationEntries = await _shortTermMemory.GetBySessionAsync(sessionId, MemoryType.Activity, cancellationToken);
            var messages = conversationEntries
                .Where(e => e.Metadata?.ContainsKey("messageType") == true)
                .Select(e => JsonSerializer.Deserialize<ChatMessage>(e.Content))
                .Where(m => m != null)
                .Cast<ChatMessage>()
                .OrderBy(m => m.GetType().GetProperty("Timestamp")?.GetValue(m) as DateTimeOffset? ?? DateTimeOffset.MinValue)
                .ToList();

            // Load relevant long-term memory
            var longTermEntries = new List<MemoryEntry>();
            if (messages.Any())
            {
                var lastMessage = messages.Last();
                // Search for relevant long-term memory based on recent conversation
                longTermEntries = await _longTermMemory.SearchAsync(lastMessage.Content, limit: 5, cancellationToken: cancellationToken);
            }

            return new ConversationContext
            {
                SessionId = sessionId,
                Messages = messages,
                ShortTermMemory = conversationEntries,
                LongTermMemory = longTermEntries,
                LastActivity = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading conversation context for session {SessionId}", sessionId);
            return new ConversationContext { SessionId = sessionId };
        }
    }

    public async Task SaveContextAsync(ConversationContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Save conversation messages to short-term memory
            foreach (var message in context.Messages)
            {
                var entry = new MemoryEntry
                {
                    Id = $"{context.SessionId}:message:{Guid.NewGuid()}",
                    SessionId = context.SessionId,
                    Type = MemoryType.Activity,
                    Content = JsonSerializer.Serialize(message),
                    Timestamp = DateTimeOffset.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        ["messageType"] = "conversation",
                        ["role"] = message.Role
                    }
                };

                await _shortTermMemory.StoreAsync(entry, cancellationToken);
            }

            // Save important insights to long-term memory
            foreach (var insight in context.Insights)
            {
                var entry = new MemoryEntry
                {
                    Id = $"{context.SessionId}:insight:{Guid.NewGuid()}",
                    SessionId = context.SessionId,
                    Type = MemoryType.Plan,
                    Content = insight,
                    Timestamp = DateTimeOffset.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        ["type"] = "insight"
                    }
                };

                await _longTermMemory.StoreAsync(entry, cancellationToken);
            }

            _logger.LogInformation("Saved conversation context for session {SessionId}", context.SessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving conversation context for session {SessionId}", context.SessionId);
            throw;
        }
    }

    public async Task AddMessageAsync(string sessionId, ChatMessage message, CancellationToken cancellationToken = default)
    {
        var entry = new MemoryEntry
        {
            Id = $"{sessionId}:message:{Guid.NewGuid()}",
            SessionId = sessionId,
            Type = MemoryType.Activity,
            Content = JsonSerializer.Serialize(message),
            Timestamp = DateTimeOffset.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["messageType"] = "conversation",
                ["role"] = message.Role
            }
        };

        await _shortTermMemory.StoreAsync(entry, cancellationToken);
    }

    public async Task<List<ChatMessage>> GetRecentMessagesAsync(string sessionId, int limit = 10, CancellationToken cancellationToken = default)
    {
        var entries = await _shortTermMemory.GetBySessionAsync(sessionId, MemoryType.Activity, cancellationToken);
        return entries
            .Where(e => e.Metadata?.ContainsKey("messageType") == true && e.Metadata["messageType"].ToString() == "conversation")
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .Select(e => JsonSerializer.Deserialize<ChatMessage>(e.Content))
            .Where(m => m != null)
            .Cast<ChatMessage>()
            .Reverse() // Put back in chronological order
            .ToList();
    }
}

public record ConversationContext
{
    public required string SessionId { get; init; }
    public List<ChatMessage> Messages { get; init; } = new();
    public List<MemoryEntry> ShortTermMemory { get; init; } = new();
    public List<MemoryEntry> LongTermMemory { get; init; } = new();
    public List<string> Insights { get; init; } = new();
    public DateTimeOffset LastActivity { get; init; }
}