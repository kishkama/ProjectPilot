using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjectPilot.Agents.Contracts;

namespace ProjectPilot.Agents.Services;

public interface IParallelAgentExecutor
{
    Task<AgentResponse[]> ExecuteParallelAsync(
        AgentMessage message,
        AgentContext context,
        IEnumerable<AgentType> agentTypes,
        CancellationToken cancellationToken = default);

    Task<AgentResponse[]> ExecuteParallelAsync(
        AgentMessage message,
        AgentContext context,
        IEnumerable<string> agentNames,
        CancellationToken cancellationToken = default);
}

public class ParallelAgentExecutor : IParallelAgentExecutor
{
    private readonly IAgentRegistry _agentRegistry;
    private readonly ILogger<ParallelAgentExecutor> _logger;
    private readonly SemaphoreSlim _throttle;

    private const int MaxParallelAgents = 3;

    public ParallelAgentExecutor(
        IAgentRegistry agentRegistry,
        ILogger<ParallelAgentExecutor> logger)
    {
        _agentRegistry = agentRegistry;
        _logger = logger;
        _throttle = new SemaphoreSlim(MaxParallelAgents);
    }

    public Task<AgentResponse[]> ExecuteParallelAsync(
        AgentMessage message,
        AgentContext context,
        IEnumerable<AgentType> agentTypes,
        CancellationToken cancellationToken = default)
    {
        var agents = agentTypes.Select(t => _agentRegistry.GetAgent(t)).Where(a => a != null);
        return ExecuteWithAgentsAsync(message, context, agents!, cancellationToken);
    }

    public Task<AgentResponse[]> ExecuteParallelAsync(
        AgentMessage message,
        AgentContext context,
        IEnumerable<string> agentNames,
        CancellationToken cancellationToken = default)
    {
        var agents = agentNames.Select(n => _agentRegistry.GetAgentByName(n)).Where(a => a != null);
        return ExecuteWithAgentsAsync(message, context, agents!, cancellationToken);
    }

    private async Task<AgentResponse[]> ExecuteWithAgentsAsync(
        AgentMessage message,
        AgentContext context,
        IEnumerable<IAgent> agents,
        CancellationToken cancellationToken)
    {
        var agentList = agents.ToList();

        _logger.LogInformation(
            "Executing {Count} agents in parallel: {Agents}",
            agentList.Count,
            string.Join(", ", agentList.Select(a => a.Name)));

        var tasks = agentList.Select(async agent =>
        {
            await _throttle.WaitAsync(cancellationToken);
            try
            {
                return await agent.ProcessAsync(message, context, cancellationToken);
            }
            finally
            {
                _throttle.Release();
            }
        });

        return await Task.WhenAll(tasks);
    }
}