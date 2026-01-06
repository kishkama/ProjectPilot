using Microsoft.Extensions.Options;

namespace ProjectPilot.LLM;

public interface IMemoryStoreFactory
{
    IMemoryStore CreateShortTermStore();
    IMemoryStore CreateLongTermStore();
    IMemoryStore GetDefaultStore();
}

public class MemoryStoreFactory : IMemoryStoreFactory
{
    private readonly RedisMemoryStore _redisStore;
    private readonly CosmosMemoryStore _cosmosStore;
    private readonly InMemoryMemoryStore _inMemoryStore;
    private readonly MemoryOptions _options;

    public MemoryStoreFactory(
        RedisMemoryStore redisStore,
        CosmosMemoryStore cosmosStore,
        InMemoryMemoryStore inMemoryStore,
        IOptions<MemoryOptions> options)
    {
        _redisStore = redisStore;
        _cosmosStore = cosmosStore;
        _inMemoryStore = inMemoryStore;
        _options = options.Value;
    }

    public IMemoryStore CreateShortTermStore() => _inMemoryStore;

    public IMemoryStore CreateLongTermStore() => _inMemoryStore;

    public IMemoryStore GetDefaultStore()
    {
        return _options.DefaultStore.ToLowerInvariant() switch
        {
            "redis" => _redisStore,
            "cosmos" => _cosmosStore,
            "inmemory" => _inMemoryStore,
            _ => _inMemoryStore
        };
    }
}