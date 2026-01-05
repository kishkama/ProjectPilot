using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectPilot.LLM;

public static class LLMServiceCollectionExtensions
{
    public static IServiceCollection AddLLMProviders(this IServiceCollection services, IConfiguration configuration)
    {
        // Register options
        services.Configure<LLMOptions>(options =>
        {
            configuration.GetSection(LLMOptions.SectionName).Bind(options);
        });
        services.Configure<AzureOpenAIOptions>(options =>
        {
            configuration.GetSection($"{LLMOptions.SectionName}:AzureOpenAI").Bind(options);
        });
        services.Configure<GeminiOptions>(options =>
        {
            configuration.GetSection($"{LLMOptions.SectionName}:Gemini").Bind(options);
        });
        services.Configure<MemoryOptions>(options =>
        {
            configuration.GetSection($"{LLMOptions.SectionName}:Memory").Bind(options);
        });
        services.Configure<RedisMemoryOptions>(options =>
        {
            configuration.GetSection($"{LLMOptions.SectionName}:Memory:Redis").Bind(options);
        });
        services.Configure<CosmosMemoryOptions>(options =>
        {
            configuration.GetSection($"{LLMOptions.SectionName}:Memory:Cosmos").Bind(options);
        });

        // Register providers
        services.AddSingleton<AzureOpenAIProvider>();
        services.AddSingleton<GeminiProvider>();

        // Register factory
        services.AddSingleton<ILLMProviderFactory, LLMProviderFactory>();

        // Register memory stores
        services.AddSingleton<RedisMemoryStore>();
        services.AddSingleton<CosmosMemoryStore>();

        // Register memory store factory
        services.AddSingleton<IMemoryStoreFactory, MemoryStoreFactory>();

        // Register conversation context manager
        services.AddSingleton<IConversationContextManager, ConversationContextManager>();

        return services;
    }
}