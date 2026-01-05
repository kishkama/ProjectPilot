using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ProjectPilot.LLM;

public class RedisMemoryStore : IMemoryStore
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisMemoryStore> _logger;
    private readonly RedisMemoryOptions _options;

    public RedisMemoryStore(IOptions<RedisMemoryOptions> options, ILogger<RedisMemoryStore> logger)
    {
        _options = options.Value;
        _logger = logger;

        var connection = ConnectionMultiplexer.Connect(_options.ConnectionString);
        _database = connection.GetDatabase();
    }

    public async Task StoreAsync(MemoryEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetKey(entry.Id);
            var serializedEntry = JsonSerializer.Serialize(entry);
            var expiry = _options.DefaultExpiry ?? TimeSpan.FromHours(24); // Default 24 hours for short-term

            await _database.StringSetAsync(key, serializedEntry, expiry, When.Always, CommandFlags.None);

            // Also store in session index for efficient retrieval
            var sessionKey = GetSessionKey(entry.SessionId, entry.Type);
            await _database.SetAddAsync(sessionKey, entry.Id);

            _logger.LogInformation("Stored memory entry {EntryId} in Redis", entry.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing memory entry {EntryId}", entry.Id);
            throw;
        }
    }

    public async Task<MemoryEntry?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var redisKey = GetKey(key);
            var value = await _database.StringGetAsync(redisKey);

            if (value.IsNullOrEmpty)
            {
                return null;
            }

            var entry = JsonSerializer.Deserialize<MemoryEntry>(value.ToString());
            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving memory entry {Key}", key);
            throw;
        }
    }

    public async Task<List<MemoryEntry>> SearchAsync(string query, string? projectId = null, int limit = 10, CancellationToken cancellationToken = default)
    {
        // Redis doesn't have built-in text search, so we'll implement a simple approach
        // In a real implementation, you might use Redisearch or another approach
        // For now, return empty list as this is primarily for long-term memory
        _logger.LogWarning("Search not implemented for Redis memory store - use Cosmos DB for search capabilities");
        return new List<MemoryEntry>();
    }

    public async Task<List<MemoryEntry>> GetBySessionAsync(string sessionId, MemoryType type, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionKey = GetSessionKey(sessionId, type);
            var entryIds = await _database.SetMembersAsync(sessionKey);

            var entries = new List<MemoryEntry>();
            foreach (var entryId in entryIds)
            {
                var entry = await GetAsync(entryId.ToString(), cancellationToken);
                if (entry != null)
                {
                    entries.Add(entry);
                }
            }

            return entries.OrderByDescending(e => e.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session memory for {SessionId}", sessionId);
            throw;
        }
    }

    public async Task DeleteBySessionAsync(string sessionId, MemoryType type, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionKey = GetSessionKey(sessionId, type);
            var entryIds = await _database.SetMembersAsync(sessionKey);

            foreach (var entryId in entryIds)
            {
                var key = GetKey(entryId.ToString());
                await _database.KeyDeleteAsync(key);
            }

            await _database.KeyDeleteAsync(sessionKey);

            _logger.LogInformation("Deleted session memory for {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session memory for {SessionId}", sessionId);
            throw;
        }
    }

    private string GetKey(string id) => $"memory:{id}";
    private string GetSessionKey(string sessionId, MemoryType type) => $"session:{sessionId}:{type}";
}

public class RedisMemoryOptions
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public TimeSpan? DefaultExpiry { get; set; }
}