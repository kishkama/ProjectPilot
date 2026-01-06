using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjectPilot.Agents.Agents;
using ProjectPilot.Agents.Configuration;
using ProjectPilot.Agents.Contracts;
using ProjectPilot.Agents.Services;
using ProjectPilot.Agents.Tools;
using ProjectPilot.LLM;

namespace ProjectPilot.Agents.DependencyInjection;

public static class AgentServiceCollectionExtensions
{
    public static IServiceCollection AddProjectPilotAgents(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register configuration
        // services.AddOptions<AgentOptions>()
        //     .Configure<IConfiguration>((options, config) =>
        //         Microsoft.Extensions.Configuration.Binder.ConfigurationBinder.Bind(options, config.GetSection(AgentOptions.SectionName)));

        // Register agents with dependencies
        services.AddSingleton<IAgent>(sp =>
        {
            var llmProviderFactory = sp.GetRequiredService<ILLMProviderFactory>();
            var llmProvider = llmProviderFactory.GetDefaultProvider();
            var agentRegistry = sp.GetRequiredService<IAgentRegistry>();
            var communicationService = sp.GetRequiredService<IAgentCommunicationService>();
            var logger = sp.GetRequiredService<ILogger<OrchestratorAgent>>();
            return new OrchestratorAgent(llmProvider, agentRegistry, communicationService, logger);
        });

        services.AddSingleton<IAgent>(sp =>
        {
            var llmProviderFactory = sp.GetRequiredService<ILLMProviderFactory>();
            var llmProvider = llmProviderFactory.GetDefaultProvider();
            var memoryStore = sp.GetRequiredService<IMemoryStore>();
            var logger = sp.GetRequiredService<ILogger<PlanningAgent>>();
            return new PlanningAgent(llmProvider, memoryStore, logger);
        });

        services.AddSingleton<IAgent>(sp =>
        {
            var llmProviderFactory = sp.GetRequiredService<ILLMProviderFactory>();
            var llmProvider = llmProviderFactory.GetDefaultProvider();
            var webSearchTool = sp.GetRequiredService<IWebSearchTool>();
            var memoryStore = sp.GetRequiredService<IMemoryStore>();
            var logger = sp.GetRequiredService<ILogger<ResearchAgent>>();
            return new ResearchAgent(llmProvider, webSearchTool, memoryStore, logger);
        });

        services.AddSingleton<IAgent>(sp =>
        {
            var llmProviderFactory = sp.GetRequiredService<ILLMProviderFactory>();
            var llmProvider = llmProviderFactory.GetDefaultProvider();
            var memoryStore = sp.GetRequiredService<IMemoryStore>();
            var logger = sp.GetRequiredService<ILogger<ReportingAgent>>();
            return new ReportingAgent(llmProvider, memoryStore, logger);
        });

        // Register agent registry
        services.AddSingleton<IAgentRegistry, AgentRegistry>();

        // Register services
        services.AddSingleton<IAgentCommunicationService, AgentCommunicationService>();
        services.AddSingleton<IAgentHandoffManager, AgentHandoffManager>();
        services.AddSingleton<IParallelAgentExecutor, ParallelAgentExecutor>();
        services.AddSingleton<IAgentStateManager, AgentStateManager>();
        services.AddSingleton<IAgentActivityLogger, AgentActivityLogger>();

        // Register external dependencies (will be provided by LLM package)
        services.AddSingleton<IWebSearchTool, WebSearchTool>();
        services.AddSingleton(sp => sp.GetRequiredService<IMemoryStoreFactory>().GetDefaultStore());

        // Register orchestration service (main entry point)
        services.AddScoped<IAgentOrchestrationService, AgentOrchestrationService>();

        return services;
    }
}