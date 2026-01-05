using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjectPilot.Agents.Contracts;
using ProjectPilot.Agents.Services;
using ProjectPilot.LLM;

namespace ProjectPilot.Agents.Agents;

public class OrchestratorAgent : BaseAgent
{
    private readonly IAgentRegistry _agentRegistry;
    private readonly IAgentCommunicationService _communicationService;

    private const string SystemPrompt = """
        You are the Orchestrator Agent for ProjectPilot, an AI-powered project management assistant.

        Your responsibilities:
        1. Analyze user requests to understand their intent
        2. Route requests to the appropriate specialist agent
        3. Coordinate multi-agent workflows when needed
        4. Synthesize responses from multiple agents

        Available specialist agents:
        - PlanningAgent: Task breakdown, prioritization, effort estimation, dependency mapping
        - ResearchAgent: Web search, documentation lookup, best practices, technology comparisons
        - ReportingAgent: Status summaries, progress metrics, risk reports, insights

        When classifying requests:
        - Planning requests: "break down", "create tasks", "plan", "prioritize", "estimate", "schedule"
        - Research requests: "find", "search", "look up", "what is", "how to", "best practice", "compare"
        - Reporting requests: "status", "summary", "report", "progress", "metrics", "blockers"

        If a request spans multiple domains, coordinate with multiple agents and synthesize their outputs.
        If unclear, ask the user for clarification.

        Always respond in a helpful, professional manner.
        """;

    public override string Name => "Orchestrator";
    public override AgentType AgentType => AgentType.Orchestrator;
    public override string Description => "Central coordinator that routes requests and manages agent workflows";

    public OrchestratorAgent(
        ILLMProvider llmProvider,
        IAgentRegistry agentRegistry,
        IAgentCommunicationService communicationService,
        ILogger<OrchestratorAgent> logger)
        : base(llmProvider, logger, "Orchestrator")
    {
        _agentRegistry = agentRegistry;
        _communicationService = communicationService;
    }

    protected override string GetSystemPrompt() => SystemPrompt;

    public override async Task<AgentResponse> ProcessAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        _logger.LogInformation(
            "Orchestrator processing message {MessageId} for session {SessionId}",
            message.Id, context.SessionId);

        // Step 1: Classify the user's intent
        var classification = await ClassifyIntentAsync(message, context, cancellationToken);

        _logger.LogInformation(
            "Intent classified as {Intent} with confidence {Confidence}",
            classification.PrimaryIntent, classification.Confidence);

        // Step 2: Route to appropriate agent(s)
        AgentResponse response;

        if (classification.RequiresMultipleAgents)
        {
            response = await CoordinateMultiAgentResponseAsync(
                message, context, classification, cancellationToken);
        }
        else
        {
            response = await DelegateToAgentAsync(
                message, context, classification.PrimaryIntent, cancellationToken);
        }

        stopwatch.Stop();

        return response with { DurationMs = stopwatch.ElapsedMilliseconds };
    }

    private async Task<IntentClassification> ClassifyIntentAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken)
    {
        var classificationPrompt = $$"""
            Classify the following user request into one or more categories.

            User request: {{message.Content}}

            Categories:
            - planning: Task breakdown, prioritization, estimation, scheduling
            - research: Information lookup, documentation, best practices
            - reporting: Status updates, metrics, progress summaries
            - general: General questions, clarifications, greetings

            Respond in JSON format:
            {
                "primaryIntent": "planning|research|reporting|general",
                "secondaryIntents": ["intent1", "intent2"],
                "confidence": 0.0-1.0,
                "reasoning": "brief explanation"
            }
            """;

        var response = await _llmProvider.ChatAsync(new ChatRequest
        {
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = SystemPrompt },
                new() { Role = "user", Content = classificationPrompt }
            },
            Temperature = 0.1f, // Low temperature for consistent classification
            MaxTokens = 200
        }, cancellationToken);

        return JsonSerializer.Deserialize<IntentClassification>(response.Content)
            ?? new IntentClassification { PrimaryIntent = "general", Confidence = 0.5f };
    }

    private async Task<AgentResponse> DelegateToAgentAsync(
        AgentMessage message,
        AgentContext context,
        string intent,
        CancellationToken cancellationToken)
    {
        var targetAgent = intent switch
        {
            "planning" => _agentRegistry.GetAgent(AgentType.Planning),
            "research" => _agentRegistry.GetAgent(AgentType.Research),
            "reporting" => _agentRegistry.GetAgent(AgentType.Reporting),
            _ => null
        };

        if (targetAgent == null)
        {
            // Handle general requests directly
            return await HandleGeneralRequestAsync(message, context, cancellationToken);
        }

        _logger.LogInformation(
            "Delegating to {AgentName} for intent {Intent}",
            targetAgent.Name, intent);

        // Send message to target agent
        var agentResponse = await targetAgent.ProcessAsync(message, context, cancellationToken);

        // Log the handoff
        await _communicationService.LogHandoffAsync(
            fromAgent: Name,
            toAgent: targetAgent.Name,
            context: context,
            cancellationToken: cancellationToken);

        return agentResponse;
    }

    private async Task<AgentResponse> CoordinateMultiAgentResponseAsync(
        AgentMessage message,
        AgentContext context,
        IntentClassification classification,
        CancellationToken cancellationToken)
    {
        var allIntents = new[] { classification.PrimaryIntent }
            .Concat(classification.SecondaryIntents ?? Array.Empty<string>())
            .Distinct()
            .ToList();

        _logger.LogInformation(
            "Coordinating multi-agent response for intents: {Intents}",
            string.Join(", ", allIntents));

        // Execute agents in parallel where possible
        var tasks = allIntents
            .Select(intent => DelegateToAgentAsync(message, context, intent, cancellationToken))
            .ToList();

        var responses = await Task.WhenAll(tasks);

        // Synthesize responses
        return await SynthesizeResponsesAsync(
            message, context, responses, cancellationToken);
    }

    private async Task<AgentResponse> SynthesizeResponsesAsync(
        AgentMessage message,
        AgentContext context,
        AgentResponse[] responses,
        CancellationToken cancellationToken)
    {
        var synthesisPrompt = $"""
            You received responses from multiple specialist agents for the user's request.
            Synthesize these into a coherent, unified response.

            User request: {message.Content}

            Agent responses:
            {string.Join("\n\n", responses.Select(r => $"[{r.FromAgent}]: {r.Content}"))}

            Create a unified response that:
            1. Integrates information from all agents
            2. Maintains a coherent narrative
            3. Avoids redundancy
            4. Clearly attributes specialized information to its source when relevant
            """;

        var synthesized = await _llmProvider.ChatAsync(new ChatRequest
        {
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = SystemPrompt },
                new() { Role = "user", Content = synthesisPrompt }
            }
        }, cancellationToken);

        return new AgentResponse
        {
            Content = synthesized.Content,
            FromAgent = Name,
            Usage = synthesized.Usage != null ? new TokenUsage
            {
                PromptTokens = (int)(synthesized.Usage.InputTokenCount ?? 0),
                CompletionTokens = (int)(synthesized.Usage.OutputTokenCount ?? 0),
                TotalTokens = (int)(synthesized.Usage.TotalTokenCount ?? 0)
            } : null
        };
    }

    private async Task<AgentResponse> HandleGeneralRequestAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken)
    {
        var response = await _llmProvider.ChatAsync(new ChatRequest
        {
            Messages = BuildConversationMessages(context, message, SystemPrompt)
        }, cancellationToken);

        return new AgentResponse
        {
            Content = response.Content,
            FromAgent = Name,
            Usage = response.Usage != null ? new TokenUsage
            {
                PromptTokens = (int)(response.Usage.InputTokenCount ?? 0),
                CompletionTokens = (int)(response.Usage.OutputTokenCount ?? 0),
                TotalTokens = (int)(response.Usage.TotalTokenCount ?? 0)
            } : null
        };
    }

    public override async IAsyncEnumerable<AgentResponseChunk> ProcessStreamAsync(
        AgentMessage message,
        AgentContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Classify intent first (non-streaming)
        var classification = await ClassifyIntentAsync(message, context, cancellationToken);

        // Emit thinking chunk
        yield return new AgentResponseChunk
        {
            Content = $"Routing to {classification.PrimaryIntent} specialist...",
            FromAgent = Name,
            Type = ChunkType.Thinking
        };

        // Get target agent
        var targetAgent = classification.PrimaryIntent switch
        {
            "planning" => _agentRegistry.GetAgent(AgentType.Planning),
            "research" => _agentRegistry.GetAgent(AgentType.Research),
            "reporting" => _agentRegistry.GetAgent(AgentType.Reporting),
            _ => null
        };

        if (targetAgent != null)
        {
            // Emit handoff chunk
            yield return new AgentResponseChunk
            {
                Content = targetAgent.Name,
                FromAgent = Name,
                Type = ChunkType.Handoff
            };

            // Stream from target agent
            await foreach (var chunk in targetAgent.ProcessStreamAsync(message, context, cancellationToken))
            {
                yield return chunk;
            }
        }
        else
        {
            // Handle general request with streaming
            await foreach (var chunk in _llmProvider.ChatStreamAsync(new ChatRequest
            {
                Messages = BuildConversationMessages(context, message, SystemPrompt)
            }, cancellationToken))
            {
                yield return new AgentResponseChunk
                {
                    Content = chunk.Content,
                    FromAgent = Name,
                    IsComplete = chunk.IsComplete
                };
            }
        }
    }
}

// Supporting types
public record IntentClassification
{
    public required string PrimaryIntent { get; init; }
    public string[]? SecondaryIntents { get; init; }
    public float Confidence { get; init; }
    public string? Reasoning { get; init; }
    public bool RequiresMultipleAgents => SecondaryIntents?.Length > 0;
}