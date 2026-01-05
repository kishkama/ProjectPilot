using System.Threading;
using System.Threading.Tasks;

namespace ProjectPilot.Agents.Contracts;

/// <summary>
/// Interface for tools that agents can invoke
/// </summary>
public interface IAgentTool
{
    /// <summary>
    /// Unique name of the tool
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description of what the tool does (for LLM)
    /// </summary>
    string Description { get; }

    /// <summary>
    /// JSON schema for tool parameters
    /// </summary>
    string ParameterSchema { get; }

    /// <summary>
    /// Execute the tool with given parameters
    /// </summary>
    Task<ToolResult> ExecuteAsync(
        string parameters,
        AgentContext context,
        CancellationToken cancellationToken = default);
}

public record ToolResult
{
    public required bool Success { get; init; }
    public required string Output { get; init; }
    public string? Error { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

public record ToolCallResult
{
    public required string ToolName { get; init; }
    public required string Parameters { get; init; }
    public required ToolResult Result { get; init; }
    public long DurationMs { get; init; }
}