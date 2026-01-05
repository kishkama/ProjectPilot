using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjectPilot.Agents.Contracts;

namespace ProjectPilot.Agents.Services;

public interface IAgentOrchestrationService
{
    Task<AgentResponse> ProcessMessageAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<AgentResponseChunk> ProcessMessageStreamAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default);
}

public class AgentOrchestrationService : IAgentOrchestrationService
{
    private readonly IAgentRegistry _agentRegistry;
    private readonly ILogger<AgentOrchestrationService> _logger;

    public AgentOrchestrationService(
        IAgentRegistry agentRegistry,
        ILogger<AgentOrchestrationService> logger)
    {
        _agentRegistry = agentRegistry;
        _logger = logger;
    }

    public async Task<AgentResponse> ProcessMessageAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing message {MessageId} for session {SessionId}",
            message.Id, context.SessionId);

        // Get the orchestrator agent
        var orchestrator = _agentRegistry.GetAgent(AgentType.Orchestrator);
        if (orchestrator == null)
        {
            throw new InvalidOperationException("Orchestrator agent not found");
        }

        // Process through orchestrator
        return await orchestrator.ProcessAsync(message, context, cancellationToken);
    }

    public IAsyncEnumerable<AgentResponseChunk> ProcessMessageStreamAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing streaming message {MessageId} for session {SessionId}",
            message.Id, context.SessionId);

        // Get the orchestrator agent
        var orchestrator = _agentRegistry.GetAgent(AgentType.Orchestrator);
        if (orchestrator == null)
        {
            throw new InvalidOperationException("Orchestrator agent not found");
        }

        // Process through orchestrator with streaming
        return orchestrator.ProcessStreamAsync(message, context, cancellationToken);
    }
}