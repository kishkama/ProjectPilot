using System.Net;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ProjectPilot.LLM;

public class CosmosMemoryStore : IMemoryStore
{
    private readonly Container _container;
    private readonly ILogger<CosmosMemoryStore> _logger;
    private readonly CosmosMemoryOptions _options;

    public CosmosMemoryStore(IOptions<CosmosMemoryOptions> options, ILogger<CosmosMemoryStore> logger)
    {
        _options = options.Value;
        _logger = logger;

        var client = new CosmosClient(_options.ConnectionString);
        var database = client.GetDatabase(_options.DatabaseName);
        _container = database.GetContainer(_options.ContainerName);
    }

    public async Task StoreAsync(MemoryEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            var cosmosEntry = new CosmosMemoryEntry
            {
                Id = entry.Id,
                SessionId = entry.SessionId,
                ProjectId = entry.ProjectId,
                Type = entry.Type,
                Content = entry.Content,
                Timestamp = entry.Timestamp,
                Metadata = entry.Metadata,
                PartitionKey = entry.SessionId // Use sessionId as partition key
            };

            await _container.UpsertItemAsync(cosmosEntry, new PartitionKey(entry.SessionId), cancellationToken: cancellationToken);

            _logger.LogInformation("Stored memory entry {EntryId} in Cosmos DB", entry.Id);
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
            var response = await _container.ReadItemAsync<CosmosMemoryEntry>(key, new PartitionKey(key.Split(':').First()), cancellationToken: cancellationToken);

            var cosmosEntry = response.Resource;
            return new MemoryEntry
            {
                Id = cosmosEntry.Id,
                SessionId = cosmosEntry.SessionId,
                ProjectId = cosmosEntry.ProjectId,
                Type = cosmosEntry.Type,
                Content = cosmosEntry.Content,
                Timestamp = cosmosEntry.Timestamp,
                Metadata = cosmosEntry.Metadata
            };
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving memory entry {Key}", key);
            throw;
        }
    }

    public async Task<List<MemoryEntry>> SearchAsync(string query, string? projectId = null, int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var sql = "SELECT * FROM c WHERE CONTAINS(c.content, @query)";
            var parameters = new List<(string Name, object Value)> { ("@query", query) };

            if (projectId != null)
            {
                sql += " AND c.projectId = @projectId";
                parameters.Add(("@projectId", projectId));
            }

            sql += " ORDER BY c.timestamp DESC OFFSET 0 LIMIT @limit";
            parameters.Add(("@limit", limit));

            var queryDefinition = new QueryDefinition(sql);
            foreach (var (name, value) in parameters)
            {
                queryDefinition.WithParameter(name, value);
            }

            var iterator = _container.GetItemQueryIterator<CosmosMemoryEntry>(queryDefinition);
            var results = new List<MemoryEntry>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                foreach (var cosmosEntry in response)
                {
                    results.Add(new MemoryEntry
                    {
                        Id = cosmosEntry.Id,
                        SessionId = cosmosEntry.SessionId,
                        ProjectId = cosmosEntry.ProjectId,
                        Type = cosmosEntry.Type,
                        Content = cosmosEntry.Content,
                        Timestamp = cosmosEntry.Timestamp,
                        Metadata = cosmosEntry.Metadata
                    });
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching memory entries with query '{Query}'", query);
            throw;
        }
    }

    public async Task<List<MemoryEntry>> GetBySessionAsync(string sessionId, MemoryType type, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.sessionId = @sessionId AND c.type = @type ORDER BY c.timestamp DESC")
                .WithParameter("@sessionId", sessionId)
                .WithParameter("@type", type);

            var iterator = _container.GetItemQueryIterator<CosmosMemoryEntry>(query);
            var results = new List<MemoryEntry>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                foreach (var cosmosEntry in response)
                {
                    results.Add(new MemoryEntry
                    {
                        Id = cosmosEntry.Id,
                        SessionId = cosmosEntry.SessionId,
                        ProjectId = cosmosEntry.ProjectId,
                        Type = cosmosEntry.Type,
                        Content = cosmosEntry.Content,
                        Timestamp = cosmosEntry.Timestamp,
                        Metadata = cosmosEntry.Metadata
                    });
                }
            }

            return results;
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
            // First get all entries for the session and type
            var entries = await GetBySessionAsync(sessionId, type, cancellationToken);

            // Delete each entry
            foreach (var entry in entries)
            {
                await _container.DeleteItemAsync<CosmosMemoryEntry>(entry.Id, new PartitionKey(sessionId), cancellationToken: cancellationToken);
            }

            _logger.LogInformation("Deleted {Count} memory entries for session {SessionId}", entries.Count, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session memory for {SessionId}", sessionId);
            throw;
        }
    }

    private class CosmosMemoryEntry
    {
        public string Id { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string? ProjectId { get; set; }
        public MemoryType Type { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
        public string PartitionKey { get; set; } = string.Empty;
    }
}

public class CosmosMemoryOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "ProjectPilot";
    public string ContainerName { get; set; } = "Memory";
}