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
    private readonly MemoryOptions _options;

    public MemoryStoreFactory(
        RedisMemoryStore redisStore,
        CosmosMemoryStore cosmosStore,
        IOptions<MemoryOptions> options)
    {
        _redisStore = redisStore;
        _cosmosStore = cosmosStore;
        _options = options.Value;
    }

    public IMemoryStore CreateShortTermStore() => _redisStore;

    public IMemoryStore CreateLongTermStore() => _cosmosStore;

    public IMemoryStore GetDefaultStore()
    {
        return _options.DefaultStore.ToLowerInvariant() switch
        {
            "redis" => _redisStore,
            "cosmos" => _cosmosStore,
            _ => _redisStore
        };
    }
}