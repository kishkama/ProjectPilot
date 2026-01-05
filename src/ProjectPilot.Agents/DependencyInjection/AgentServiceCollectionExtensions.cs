using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectPilot.Agents.Agents;
using ProjectPilot.Agents.Configuration;
using ProjectPilot.Agents.Contracts;
using ProjectPilot.Agents.Services;

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

        // Register agents
        services.AddSingleton<IAgent, OrchestratorAgent>();
        services.AddSingleton<IAgent, PlanningAgent>();
        services.AddSingleton<IAgent, ResearchAgent>();
        services.AddSingleton<IAgent, ReportingAgent>();

        // Register agent registry
        services.AddSingleton<IAgentRegistry, AgentRegistry>();

        // Register services
        services.AddSingleton<IAgentCommunicationService, AgentCommunicationService>();
        services.AddSingleton<IAgentHandoffManager, AgentHandoffManager>();
        services.AddSingleton<IParallelAgentExecutor, ParallelAgentExecutor>();
        services.AddSingleton<IAgentStateManager, AgentStateManager>();
        services.AddSingleton<IAgentActivityLogger, AgentActivityLogger>();

        // Register orchestration service (main entry point)
        services.AddScoped<IAgentOrchestrationService, AgentOrchestrationService>();

        return services;
    }
}