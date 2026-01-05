using System.Collections.Generic;
using ProjectPilot.Agents.Contracts;

namespace ProjectPilot.Agents.Services;

public interface IAgentRegistry
{
    IAgent? GetAgent(AgentType type);
    IAgent? GetAgentByName(string name);
    IEnumerable<IAgent> GetAllAgents();
    void RegisterAgent(IAgent agent);
}

public class AgentRegistry : IAgentRegistry
{
    private readonly Dictionary<AgentType, IAgent> _agentsByType = new();
    private readonly Dictionary<string, IAgent> _agentsByName = new(StringComparer.OrdinalIgnoreCase);

    public AgentRegistry(IEnumerable<IAgent> agents)
    {
        foreach (var agent in agents)
        {
            RegisterAgent(agent);
        }
    }

    public IAgent? GetAgent(AgentType type)
    {
        return _agentsByType.TryGetValue(type, out var agent) ? agent : null;
    }

    public IAgent? GetAgentByName(string name)
    {
        return _agentsByName.TryGetValue(name, out var agent) ? agent : null;
    }

    public IEnumerable<IAgent> GetAllAgents()
    {
        return _agentsByType.Values;
    }

    public void RegisterAgent(IAgent agent)
    {
        _agentsByType[agent.AgentType] = agent;
        _agentsByName[agent.Name] = agent;
    }
}