using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjectPilot.Agents.Contracts;

namespace ProjectPilot.Agents.Services;

public interface IAgentHandoffManager
{
    Task<AgentResponse> ExecuteHandoffAsync(
        AgentResponse sourceResponse,
        AgentContext context,
        CancellationToken cancellationToken = default);

    bool ShouldHandoff(AgentResponse response);
}

public class AgentHandoffManager : IAgentHandoffManager
{
    private readonly IAgentRegistry _agentRegistry;
    private readonly IAgentCommunicationService _communicationService;
    private readonly ILogger<AgentHandoffManager> _logger;

    private const int MaxHandoffDepth = 5; // Prevent infinite loops

    public AgentHandoffManager(
        IAgentRegistry agentRegistry,
        IAgentCommunicationService communicationService,
        ILogger<AgentHandoffManager> logger)
    {
        _agentRegistry = agentRegistry;
        _communicationService = communicationService;
        _logger = logger;
    }

    public bool ShouldHandoff(AgentResponse response)
    {
        return response.RequestsHandoff && !string.IsNullOrEmpty(response.HandoffTarget);
    }

    public async Task<AgentResponse> ExecuteHandoffAsync(
        AgentResponse sourceResponse,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        var handoffDepth = GetHandoffDepth(context);

        if (handoffDepth >= MaxHandoffDepth)
        {
            _logger.LogWarning(
                "Max handoff depth {MaxDepth} reached, terminating chain",
                MaxHandoffDepth);
            return sourceResponse;
        }

        var targetAgent = _agentRegistry.GetAgentByName(sourceResponse.HandoffTarget!);
        if (targetAgent == null)
        {
            _logger.LogWarning(
                "Handoff target {Target} not found",
                sourceResponse.HandoffTarget);
            return sourceResponse;
        }

        _logger.LogInformation(
            "Executing handoff from {FromAgent} to {ToAgent}",
            sourceResponse.FromAgent, sourceResponse.HandoffTarget);

        // Create handoff message
        var handoffMessage = new AgentMessage
        {
            Content = sourceResponse.HandoffContext ?? sourceResponse.Content,
            Role = MessageRole.Agent,
            FromAgent = sourceResponse.FromAgent,
            ToAgent = sourceResponse.HandoffTarget
        };

        // Update context with incremented handoff depth
        var updatedContext = context with
        {
            Properties = new Dictionary<string, object>(context.Properties)
            {
                ["handoff_depth"] = handoffDepth + 1
            }
        };

        // Log the handoff
        await _communicationService.LogHandoffAsync(
            sourceResponse.FromAgent,
            sourceResponse.HandoffTarget!,
            updatedContext,
            cancellationToken);

        // Execute with target agent
        var targetResponse = await targetAgent.ProcessAsync(
            handoffMessage, updatedContext, cancellationToken);

        // Check if target wants to handoff too
        if (ShouldHandoff(targetResponse))
        {
            return await ExecuteHandoffAsync(targetResponse, updatedContext, cancellationToken);
        }

        return targetResponse;
    }

    private int GetHandoffDepth(AgentContext context)
    {
        if (context.Properties.TryGetValue("handoff_depth", out var depth) && depth is int intDepth)
        {
            return intDepth;
        }
        return 0;
    }
}