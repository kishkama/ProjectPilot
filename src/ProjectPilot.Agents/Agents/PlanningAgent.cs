using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjectPilot.Agents.Contracts;
using ProjectPilot.LLM;

namespace ProjectPilot.Agents.Agents;

public class PlanningAgent : BaseAgent
{
    private const string SystemPrompt = """
        You are the Planning Agent for ProjectPilot, specializing in project planning and task management.

        Your capabilities:
        1. **Goal Decomposition**: Break high-level goals into specific, actionable tasks
        2. **Task Prioritization**: Rank tasks using priority frameworks (MoSCoW, Eisenhower Matrix)
        3. **Dependency Mapping**: Identify which tasks depend on others
        4. **Effort Estimation**: Provide relative complexity estimates (T-shirt sizing: XS, S, M, L, XL)
        5. **Milestone Planning**: Group tasks into logical milestones

        When creating tasks:
        - Each task should be specific and actionable
        - Include clear acceptance criteria when possible
        - Identify blockers and dependencies
        - Suggest logical ordering

        Output format for task breakdowns:
        ```
        ## Task: [Task Name]
        - **Priority**: High/Medium/Low
        - **Effort**: XS/S/M/L/XL
        - **Dependencies**: [List of dependent tasks]
        - **Description**: [What needs to be done]
        - **Acceptance Criteria**: [How to know it's done]
        ```

        Be thorough but practical. Focus on delivering value incrementally.
        """;

    public override string Name => "Planning";
    public override AgentType AgentType => AgentType.Planning;
    public override string Description => "Specializes in task breakdown, prioritization, and project planning";

    private readonly IMemoryStore _memoryStore;

    public PlanningAgent(
        ILLMProvider llmProvider,
        IMemoryStore memoryStore,
        ILogger<PlanningAgent> logger)
        : base(llmProvider, logger, "Planning")
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
            "Planning Agent processing request for session {SessionId}",
            context.SessionId);

        // Retrieve relevant project context from memory
        var projectMemory = await RetrieveProjectContextAsync(context, cancellationToken);

        // Build the planning prompt
        var planningPrompt = BuildPlanningPrompt(message, context, projectMemory);

        // Generate the plan
        var response = await _llmProvider.ChatAsync(new ChatRequest
        {
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = SystemPrompt },
                new() { Role = "user", Content = planningPrompt }
            },
            Temperature = 0.7f,
            MaxTokens = 2000
        }, cancellationToken);

        // Store the generated plan in memory for future reference
        await StorePlanInMemoryAsync(context, response.Content, cancellationToken);

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

    private async Task<List<ProjectPilot.LLM.MemoryEntry>> RetrieveProjectContextAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(context.ProjectId))
            return new List<ProjectPilot.LLM.MemoryEntry>();

        return await _memoryStore.SearchAsync(
            query: "project goals tasks milestones",
            projectId: context.ProjectId,
            limit: 5,
            cancellationToken: cancellationToken);
    }

    private string BuildPlanningPrompt(
        AgentMessage message,
        AgentContext context,
        List<ProjectPilot.LLM.MemoryEntry> projectMemory)
    {
        var promptBuilder = new System.Text.StringBuilder();

        // Add project context if available
        if (projectMemory.Any())
        {
            promptBuilder.AppendLine("## Existing Project Context");
            foreach (var memory in projectMemory)
            {
                promptBuilder.AppendLine($"- {memory.Content}");
            }
            promptBuilder.AppendLine();
        }

        // Add conversation context
        if (context.ConversationHistory.Any())
        {
            promptBuilder.AppendLine("## Recent Conversation");
            foreach (var msg in context.ConversationHistory.TakeLast(5))
            {
                promptBuilder.AppendLine($"[{msg.Role}]: {msg.Content}");
            }
            promptBuilder.AppendLine();
        }

        // Add the current request
        promptBuilder.AppendLine("## Current Request");
        promptBuilder.AppendLine(message.Content);

        return promptBuilder.ToString();
    }

    private async Task StorePlanInMemoryAsync(
        AgentContext context,
        string planContent,
        CancellationToken cancellationToken)
    {
        var memoryEntry = new ProjectPilot.LLM.MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = context.SessionId,
            ProjectId = context.ProjectId,
            Type = ProjectPilot.LLM.MemoryType.Plan,
            Content = planContent,
            Timestamp = DateTimeOffset.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["agent"] = Name,
                ["type"] = "task_breakdown"
            }
        };

        await _memoryStore.StoreAsync(memoryEntry, cancellationToken);
    }

    public override async IAsyncEnumerable<AgentResponseChunk> ProcessStreamAsync(
        AgentMessage message,
        AgentContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var projectMemory = await RetrieveProjectContextAsync(context, cancellationToken);
        var planningPrompt = BuildPlanningPrompt(message, context, projectMemory);

        var fullContent = new System.Text.StringBuilder();

        await foreach (var chunk in _llmProvider.ChatStreamAsync(new ChatRequest
        {
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = SystemPrompt },
                new() { Role = "user", Content = planningPrompt }
            },
            Temperature = 0.7f,
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

        // Store completed plan in memory
        await StorePlanInMemoryAsync(context, fullContent.ToString(), cancellationToken);
    }
}