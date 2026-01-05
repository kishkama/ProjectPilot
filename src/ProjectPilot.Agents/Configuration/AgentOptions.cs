namespace ProjectPilot.Agents.Configuration;

public class AgentOptions
{
    public const string SectionName = "Agents";

    /// <summary>
    /// Configuration for the Orchestrator agent
    /// </summary>
    public OrchestratorAgentOptions Orchestrator { get; set; } = new();

    /// <summary>
    /// Configuration for the Planning agent
    /// </summary>
    public PlanningAgentOptions Planning { get; set; } = new();

    /// <summary>
    /// Configuration for the Research agent
    /// </summary>
    public ResearchAgentOptions Research { get; set; } = new();

    /// <summary>
    /// Configuration for the Reporting agent
    /// </summary>
    public ReportingAgentOptions Reporting { get; set; } = new();

    /// <summary>
    /// Global agent settings
    /// </summary>
    public GlobalAgentSettings Global { get; set; } = new();
}

public class BaseAgentOptions
{
    /// <summary>
    /// Whether this agent is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Custom system prompt (overrides default)
    /// </summary>
    public string? CustomSystemPrompt { get; set; }

    /// <summary>
    /// LLM temperature for this agent
    /// </summary>
    public float Temperature { get; set; } = 0.7f;

    /// <summary>
    /// Max tokens for responses
    /// </summary>
    public int MaxTokens { get; set; } = 2000;
}

public class OrchestratorAgentOptions : BaseAgentOptions
{
    /// <summary>
    /// Enable parallel agent execution
    /// </summary>
    public bool EnableParallelExecution { get; set; } = true;

    /// <summary>
    /// Max handoff depth to prevent infinite loops
    /// </summary>
    public int MaxHandoffDepth { get; set; } = 5;
}

public class PlanningAgentOptions : BaseAgentOptions
{
    /// <summary>
    /// Default effort estimation scale
    /// </summary>
    public string EstimationScale { get; set; } = "TShirt"; // TShirt, Fibonacci, Hours
}

public class ResearchAgentOptions : BaseAgentOptions
{
    /// <summary>
    /// Max search results to consider
    /// </summary>
    public int MaxSearchResults { get; set; } = 10;

    /// <summary>
    /// Enable web search
    /// </summary>
    public bool EnableWebSearch { get; set; } = true;
}

public class ReportingAgentOptions : BaseAgentOptions
{
    /// <summary>
    /// Default report format
    /// </summary>
    public string DefaultFormat { get; set; } = "Markdown";
}

public class GlobalAgentSettings
{
    /// <summary>
    /// Default timeout for agent operations (ms)
    /// </summary>
    public int TimeoutMs { get; set; } = 60000;

    /// <summary>
    /// Enable activity logging
    /// </summary>
    public bool EnableActivityLogging { get; set; } = true;

    /// <summary>
    /// Max conversation history to include in context
    /// </summary>
    public int MaxConversationHistory { get; set; } = 10;
}