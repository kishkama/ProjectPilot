using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ProjectPilot.LLM;

public interface ILLMProviderFactory
{
    ILLMProvider CreateProvider(string providerName);
    ILLMProvider GetDefaultProvider();
}

public class LLMProviderFactory : ILLMProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly LLMOptions _options;

    public LLMProviderFactory(IServiceProvider serviceProvider, IOptions<LLMOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    public ILLMProvider CreateProvider(string providerName)
    {
        return providerName.ToLowerInvariant() switch
        {
            "azureopenai" => _serviceProvider.GetRequiredService<AzureOpenAIProvider>(),
            "gemini" => _serviceProvider.GetRequiredService<GeminiProvider>(),
            _ => throw new ArgumentException($"Unknown provider: {providerName}", nameof(providerName))
        };
    }

    public ILLMProvider GetDefaultProvider()
    {
        return CreateProvider(_options.DefaultProvider);
    }
}