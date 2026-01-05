using System;

namespace ProjectPilot.Agents.Exceptions;

public class AgentException : Exception
{
    public AgentException(string message) : base(message) { }
    public AgentException(string message, Exception innerException) : base(message, innerException) { }
}

public class AgentNotFoundException : AgentException
{
    public AgentNotFoundException(string agentName)
        : base($"Agent '{agentName}' not found in registry")
    {
    }
}

public class AgentCommunicationException : AgentException
{
    public AgentCommunicationException(string message) : base(message) { }
    public AgentCommunicationException(string message, Exception innerException) : base(message, innerException) { }
}

public class AgentHandoffException : AgentException
{
    public AgentHandoffException(string message) : base(message) { }
    public AgentHandoffException(string message, Exception innerException) : base(message, innerException) { }
}