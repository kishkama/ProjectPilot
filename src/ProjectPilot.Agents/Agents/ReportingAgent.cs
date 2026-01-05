using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjectPilot.Agents.Contracts;
using ProjectPilot.LLM;


namespace ProjectPilot.Agents.Agents;

public class ReportingAgent : BaseAgent
{
    private const string SystemPrompt = """
        You are the Reporting Agent for ProjectPilot, specializing in project status and metrics.

        Your capabilities:
        1. **Status Summaries**: Generate clear project status reports
        2. **Progress Metrics**: Calculate and present progress percentages
        3. **Blocker Identification**: List current blockers and impediments
        4. **Risk Assessment**: Identify and categorize project risks
        5. **Insights & Recommendations**: Provide actionable recommendations

        Report Format:
        ```
        ## Project Status Report
        **Generated**: [Date]
        **Project**: [Name]

        ### Executive Summary
        [2-3 sentence overview]

        ### Progress
        - Overall: [X]% complete
        - [Milestone 1]: [Status]
        - [Milestone 2]: [Status]

        ### Current Blockers
        1. [Blocker with impact]
        2. [Blocker with impact]

        ### Risks
        | Risk | Likelihood | Impact | Mitigation |
        |------|------------|--------|------------|

        ### Recommendations
        1. [Actionable recommendation]
        2. [Actionable recommendation]
        ```

        Be data-driven when possible. Highlight both achievements and concerns.
        """;

    public override string Name => "Reporting";
    public override AgentType AgentType => AgentType.Reporting;
    public override string Description => "Specializes in status reports, metrics, and project insights";

    private readonly IMemoryStore _memoryStore;

    public ReportingAgent(
        ILLMProvider llmProvider,
        IMemoryStore memoryStore,
        ILogger<ReportingAgent> logger)
        : base(llmProvider, logger, "Reporting")
    {
        _memoryStore = memoryStore;
    }

    protected override string GetSystemPrompt() => SystemPrompt;

    public override async Task<AgentResponse> ProcessAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        _logger.LogInformation(
            "Reporting Agent processing request for session {SessionId}",
            context.SessionId);

        // Gather project data from memory
        var projectData = await GatherProjectDataAsync(context, cancellationToken);

        // Build reporting prompt
        var reportingPrompt = BuildReportingPrompt(message, context, projectData);

        // Generate report
        var response = await _llmProvider.ChatAsync(new ChatRequest
        {
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = SystemPrompt },
                new() { Role = "user", Content = reportingPrompt }
            },
            Temperature = 0.5f,
            MaxTokens = 2000
        }, cancellationToken);

        // Store report in memory
        await StoreReportInMemoryAsync(context, response.Content, cancellationToken);

        stopwatch.Stop();

        return new AgentResponse
        {
            Content = response.Content,
            FromAgent = Name,
            Usage = response.Usage != null ? new TokenUsage
            {
                PromptTokens = (int)(response.Usage.InputTokenCount ?? 0),
                CompletionTokens = (int)(response.Usage.OutputTokenCount ?? 0),
                TotalTokens = (int)(response.Usage.TotalTokenCount ?? 0)
            } : null,
            DurationMs = stopwatch.ElapsedMilliseconds
        };
    }

    private async Task<ProjectData> GatherProjectDataAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        var projectData = new ProjectData();

        // Get plans from memory
        var plans = await _memoryStore.GetBySessionAsync(
            context.SessionId,
            ProjectPilot.LLM.MemoryType.Plan,
            cancellationToken);

        projectData.Plans = plans;

        // Get previous reports
        var previousReports = await _memoryStore.GetBySessionAsync(
            context.SessionId,
            ProjectPilot.LLM.MemoryType.Report,
            cancellationToken);

        projectData.PreviousReports = previousReports;

        // Get research findings
        var research = await _memoryStore.GetBySessionAsync(
            context.SessionId,
            ProjectPilot.LLM.MemoryType.Research,
            cancellationToken);

        projectData.Research = research;

        return projectData;
    }

    private string BuildReportingPrompt(
        AgentMessage message,
        AgentContext context,
        ProjectData projectData)
    {
        var promptBuilder = new System.Text.StringBuilder();

        promptBuilder.AppendLine("## Report Request");
        promptBuilder.AppendLine(message.Content);
        promptBuilder.AppendLine();

        if (projectData.Plans.Any())
        {
            promptBuilder.AppendLine("## Project Plans");
            foreach (var plan in projectData.Plans)
            {
                promptBuilder.AppendLine($"### Plan from {plan.Timestamp:g}");
                promptBuilder.AppendLine(plan.Content);
                promptBuilder.AppendLine();
            }
        }

        if (projectData.PreviousReports.Any())
        {
            promptBuilder.AppendLine("## Previous Reports");
            foreach (var report in projectData.PreviousReports.Take(2))
            {
                promptBuilder.AppendLine($"### Report from {report.Timestamp:g}");
                promptBuilder.AppendLine(report.Content);
                promptBuilder.AppendLine();
            }
        }

        promptBuilder.AppendLine("## Conversation History");
        foreach (var msg in context.ConversationHistory.TakeLast(5))
        {
            promptBuilder.AppendLine($"[{msg.Role}]: {msg.Content}");
        }

        return promptBuilder.ToString();
    }

    private async Task StoreReportInMemoryAsync(
        AgentContext context,
        string reportContent,
        CancellationToken cancellationToken)
    {
        var memoryEntry = new ProjectPilot.LLM.MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = context.SessionId,
            ProjectId = context.ProjectId,
            Type = ProjectPilot.LLM.MemoryType.Report,
            Content = reportContent,
            Timestamp = DateTimeOffset.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["agent"] = Name,
                ["type"] = "status_report"
            }
        };

        await _memoryStore.StoreAsync(memoryEntry, cancellationToken);
    }

    public override async IAsyncEnumerable<AgentResponseChunk> ProcessStreamAsync(
        AgentMessage message,
        AgentContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return new AgentResponseChunk
        {
            Content = "Gathering project data...\n\n",
            FromAgent = Name,
            Type = ChunkType.Thinking
        };

        var projectData = await GatherProjectDataAsync(context, cancellationToken);
        var reportingPrompt = BuildReportingPrompt(message, context, projectData);

        var fullContent = new System.Text.StringBuilder();

        await foreach (var chunk in _llmProvider.ChatStreamAsync(new ChatRequest
        {
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = SystemPrompt },
                new() { Role = "user", Content = reportingPrompt }
            },
            Temperature = 0.5f,
            MaxTokens = 2000
        }, cancellationToken))
        {
            fullContent.Append(chunk.Content);

            yield return new AgentResponseChunk
            {
                Content = chunk.Content,
                FromAgent = Name,
                IsComplete = chunk.IsComplete
            };
        }

        await StoreReportInMemoryAsync(context, fullContent.ToString(), cancellationToken);
    }
}

// Supporting type
public class ProjectData
{
    public List<ProjectPilot.LLM.MemoryEntry> Plans { get; set; } = new();
    public List<ProjectPilot.LLM.MemoryEntry> PreviousReports { get; set; } = new();
    public List<ProjectPilot.LLM.MemoryEntry> Research { get; set; } = new();
}