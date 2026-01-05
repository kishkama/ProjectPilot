# Implementation Guide: Multi-Agent System

**Feature Category:** 1 - Multi-Agent System
**Document Version:** 1.0
**Last Updated:** 18-Dec-2025

---

## Table of Contents

1. [Overview](#1-overview)
2. [Architecture](#2-architecture)
3. [AutoGen.Net Integration](#3-autogennet-integration)
4. [Agent Interfaces & Contracts](#4-agent-interfaces--contracts)
5. [Orchestrator Agent Implementation](#5-orchestrator-agent-implementation)
6. [Planning Agent Implementation](#6-planning-agent-implementation)
7. [Research Agent Implementation](#7-research-agent-implementation)
8. [Reporting Agent Implementation](#8-reporting-agent-implementation)
9. [Agent Communication Protocol](#9-agent-communication-protocol)
10. [Agent Handoff Mechanism](#10-agent-handoff-mechanism)
11. [Parallel Agent Execution](#11-parallel-agent-execution)
12. [State Management](#12-state-management)
13. [Activity Logging](#13-activity-logging)
14. [Configuration](#14-configuration)
15. [Dependency Injection Setup](#15-dependency-injection-setup)
16. [Testing Strategy](#16-testing-strategy)
17. [Error Handling](#17-error-handling)
18. [Performance Considerations](#18-performance-considerations)

---

## 1. Overview

### 1.1 Purpose

The Multi-Agent System is the core intelligence layer of ProjectPilot. It consists of multiple specialized AI agents that collaborate to handle complex project management tasks. Each agent has a specific role and expertise, coordinated by a central Orchestrator Agent.

### 1.2 Features Covered

| Feature ID | Feature Name | Priority | Status |
|------------|--------------|----------|--------|
| MA-001 | Orchestrator Agent | P0 | To Implement |
| MA-002 | Agent Communication | P0 | To Implement |
| MA-003 | Agent Handoff | P0 | To Implement |
| MA-004 | Parallel Agent Execution | P1 | To Implement |
| MA-005 | Agent State Management | P1 | To Implement |
| MA-006 | Agent Activity Logging | P1 | To Implement |
| MA-007 | Custom Agent Prompts | P2 | To Implement |

### 1.3 Dependencies

- **AutoGen.Net** (v0.2.x) - Multi-agent orchestration framework
- **ProjectPilot.LLM** - LLM provider abstraction layer
- **ProjectPilot.Memory** - Short-term and long-term memory stores
- **Microsoft.Extensions.Logging** - Structured logging
- **Microsoft.Extensions.DependencyInjection** - DI container

---

## 2. Architecture

### 2.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              API Layer                                   │
│                         (Chat Endpoints)                                 │
└─────────────────────────────────┬───────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         Agent Service Layer                              │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                      AgentOrchestrationService                     │  │
│  │         (Entry point, manages agent lifecycle & sessions)          │  │
│  └───────────────────────────────┬───────────────────────────────────┘  │
│                                  │                                       │
│  ┌───────────────────────────────▼───────────────────────────────────┐  │
│  │                       OrchestratorAgent                            │  │
│  │              (Routes requests, coordinates workflows)              │  │
│  └───────────────────────────────┬───────────────────────────────────┘  │
│                                  │                                       │
│            ┌─────────────────────┼─────────────────────┐                │
│            │                     │                     │                │
│            ▼                     ▼                     ▼                │
│  ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐        │
│  │  PlanningAgent   │ │  ResearchAgent   │ │  ReportingAgent  │        │
│  │                  │ │                  │ │                  │        │
│  │ • Task breakdown │ │ • Web search     │ │ • Status reports │        │
│  │ • Prioritization │ │ • Doc lookup     │ │ • Metrics        │        │
│  │ • Dependencies   │ │ • Best practices │ │ • Insights       │        │
│  └──────────────────┘ └──────────────────┘ └──────────────────┘        │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                          Infrastructure Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐         │
│  │   LLM Provider  │  │  Memory Store   │  │  Tool Registry  │         │
│  │ (Azure/Gemini)  │  │ (Redis/Cosmos)  │  │  (Agent Tools)  │         │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Component Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        ProjectPilot.Agents                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                          Contracts/                              │    │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐                │    │
│  │  │ IAgent      │ │ IAgentTool  │ │ IAgentState │                │    │
│  │  └─────────────┘ └─────────────┘ └─────────────┘                │    │
│  │  ┌─────────────────────┐ ┌─────────────────────┐                │    │
│  │  │ AgentMessage        │ │ AgentContext        │                │    │
│  │  └─────────────────────┘ └─────────────────────┘                │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                           Agents/                                │    │
│  │  ┌─────────────────┐ ┌─────────────────┐                        │    │
│  │  │ BaseAgent       │ │ AgentFactory    │                        │    │
│  │  └────────┬────────┘ └─────────────────┘                        │    │
│  │           │                                                      │    │
│  │  ┌────────┴────────────────────────────────────────────┐        │    │
│  │  │                    │                │               │        │    │
│  │  ▼                    ▼                ▼               ▼        │    │
│  │ ┌──────────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐        │    │
│  │ │ Orchestrator │ │ Planning │ │ Research │ │ Reporting│        │    │
│  │ │ Agent        │ │ Agent    │ │ Agent    │ │ Agent    │        │    │
│  │ └──────────────┘ └──────────┘ └──────────┘ └──────────┘        │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                           Tools/                                 │    │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐                │    │
│  │  │ WebSearch   │ │ TaskCreate  │ │ ReportGen   │                │    │
│  │  │ Tool        │ │ Tool        │ │ Tool        │                │    │
│  │  └─────────────┘ └─────────────┘ └─────────────┘                │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                          Services/                               │    │
│  │  ┌─────────────────────────┐ ┌─────────────────────────┐        │    │
│  │  │ AgentOrchestrationSvc   │ │ AgentCommunicationSvc   │        │    │
│  │  └─────────────────────────┘ └─────────────────────────┘        │    │
│  │  ┌─────────────────────────┐ ┌─────────────────────────┐        │    │
│  │  │ AgentStateManager       │ │ AgentActivityLogger     │        │    │
│  │  └─────────────────────────┘ └─────────────────────────┘        │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.3 Sequence Diagram - Basic Request Flow

```
┌──────┐     ┌─────────┐     ┌─────────────┐     ┌──────────┐     ┌─────┐
│ User │     │   API   │     │ Orchestrator│     │ Planning │     │ LLM │
└──┬───┘     └────┬────┘     └──────┬──────┘     └────┬─────┘     └──┬──┘
   │              │                 │                 │              │
   │ "Break down  │                 │                 │              │
   │  my project" │                 │                 │              │
   │─────────────>│                 │                 │              │
   │              │                 │                 │              │
   │              │ ProcessMessage  │                 │              │
   │              │────────────────>│                 │              │
   │              │                 │                 │              │
   │              │                 │ Classify Intent │              │
   │              │                 │────────────────────────────────>
   │              │                 │                 │              │
   │              │                 │<───────────────────────────────│
   │              │                 │ "planning"      │              │
   │              │                 │                 │              │
   │              │                 │ Delegate        │              │
   │              │                 │────────────────>│              │
   │              │                 │                 │              │
   │              │                 │                 │ Generate Plan│
   │              │                 │                 │─────────────>│
   │              │                 │                 │              │
   │              │                 │                 │<─────────────│
   │              │                 │                 │   Plan       │
   │              │                 │<────────────────│              │
   │              │                 │   Plan Result   │              │
   │              │<────────────────│                 │              │
   │              │   Response      │                 │              │
   │<─────────────│                 │                 │              │
   │   Plan       │                 │                 │              │
   │              │                 │                 │              │
```

---

## 3. AutoGen.Net Integration

### 3.1 NuGet Packages

```xml
<!-- ProjectPilot.Agents.csproj -->
<ItemGroup>
  <PackageReference Include="AutoGen" Version="0.2.0" />
  <PackageReference Include="AutoGen.Core" Version="0.2.0" />
  <PackageReference Include="AutoGen.SemanticKernel" Version="0.2.0" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
  <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
</ItemGroup>
```

### 3.2 AutoGen Agent Wrapper

We wrap AutoGen agents to provide our custom abstraction:

```csharp
using AutoGen.Core;

namespace ProjectPilot.Agents.Core;

/// <summary>
/// Wrapper that bridges our IAgent interface with AutoGen's IAgent
/// </summary>
public abstract class AutoGenAgentWrapper : IAgent
{
    protected readonly IAgent _autoGenAgent;
    protected readonly ILogger _logger;
    protected readonly ILLMProvider _llmProvider;

    public string Name => _autoGenAgent.Name;
    public abstract AgentType AgentType { get; }
    public abstract string Description { get; }

    protected AutoGenAgentWrapper(
        ILLMProvider llmProvider,
        ILogger logger)
    {
        _llmProvider = llmProvider;
        _logger = logger;
    }

    public abstract Task<AgentResponse> ProcessAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default);

    public abstract IAsyncEnumerable<AgentResponseChunk> ProcessStreamAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default);
}
```

---

## 4. Agent Interfaces & Contracts

### 4.1 Core Interfaces

```csharp
// File: src/ProjectPilot.Agents/Contracts/IAgent.cs

namespace ProjectPilot.Agents.Contracts;

/// <summary>
/// Core interface for all ProjectPilot agents
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Unique name identifying this agent
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Type classification of the agent
    /// </summary>
    AgentType AgentType { get; }

    /// <summary>
    /// Human-readable description of agent capabilities
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Process a message and return a response
    /// </summary>
    Task<AgentResponse> ProcessAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a message and stream response chunks
    /// </summary>
    IAsyncEnumerable<AgentResponseChunk> ProcessStreamAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Agent type classification
/// </summary>
public enum AgentType
{
    Orchestrator,
    Planning,
    Research,
    Reporting
}
```

### 4.2 Agent Message Models

```csharp
// File: src/ProjectPilot.Agents/Contracts/AgentMessage.cs

namespace ProjectPilot.Agents.Contracts;

/// <summary>
/// Represents a message sent to or from an agent
/// </summary>
public record AgentMessage
{
    /// <summary>
    /// Unique identifier for this message
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The content of the message
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Role of the message sender
    /// </summary>
    public required MessageRole Role { get; init; }

    /// <summary>
    /// Name of the sending agent (if from an agent)
    /// </summary>
    public string? FromAgent { get; init; }

    /// <summary>
    /// Name of the target agent (if directed to specific agent)
    /// </summary>
    public string? ToAgent { get; init; }

    /// <summary>
    /// Timestamp when message was created
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optional metadata attached to the message
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

public enum MessageRole
{
    User,
    Agent,
    System,
    Tool
}
```

### 4.3 Agent Response Models

```csharp
// File: src/ProjectPilot.Agents/Contracts/AgentResponse.cs

namespace ProjectPilot.Agents.Contracts;

/// <summary>
/// Complete response from an agent
/// </summary>
public record AgentResponse
{
    /// <summary>
    /// Unique identifier for this response
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The content of the response
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Name of the agent that generated this response
    /// </summary>
    public required string FromAgent { get; init; }

    /// <summary>
    /// Whether this agent wants to hand off to another agent
    /// </summary>
    public bool RequestsHandoff { get; init; }

    /// <summary>
    /// Target agent for handoff (if RequestsHandoff is true)
    /// </summary>
    public string? HandoffTarget { get; init; }

    /// <summary>
    /// Context to pass to the handoff target
    /// </summary>
    public string? HandoffContext { get; init; }

    /// <summary>
    /// Tool calls made during response generation
    /// </summary>
    public List<ToolCallResult>? ToolCalls { get; init; }

    /// <summary>
    /// Token usage statistics
    /// </summary>
    public TokenUsage? Usage { get; init; }

    /// <summary>
    /// Processing duration in milliseconds
    /// </summary>
    public long DurationMs { get; init; }
}

/// <summary>
/// Streaming response chunk from an agent
/// </summary>
public record AgentResponseChunk
{
    public required string Content { get; init; }
    public required string FromAgent { get; init; }
    public bool IsComplete { get; init; }
    public ChunkType Type { get; init; } = ChunkType.Content;
}

public enum ChunkType
{
    Content,
    ToolCall,
    Thinking,
    Handoff
}
```

### 4.4 Agent Context

```csharp
// File: src/ProjectPilot.Agents/Contracts/AgentContext.cs

namespace ProjectPilot.Agents.Contracts;

/// <summary>
/// Context passed to agents during processing
/// </summary>
public record AgentContext
{
    /// <summary>
    /// Unique session identifier
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// User identifier (if authenticated)
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Project identifier (if in project context)
    /// </summary>
    public string? ProjectId { get; init; }

    /// <summary>
    /// Conversation history for this session
    /// </summary>
    public List<AgentMessage> ConversationHistory { get; init; } = new();

    /// <summary>
    /// Short-term memory entries relevant to this conversation
    /// </summary>
    public List<MemoryEntry> ShortTermMemory { get; init; } = new();

    /// <summary>
    /// Long-term memory entries retrieved for context
    /// </summary>
    public List<MemoryEntry> LongTermMemory { get; init; } = new();

    /// <summary>
    /// Current agent state
    /// </summary>
    public AgentState? CurrentState { get; init; }

    /// <summary>
    /// Custom properties for extensibility
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();
}
```

### 4.5 Agent Tool Interface

```csharp
// File: src/ProjectPilot.Agents/Contracts/IAgentTool.cs

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
```

---

## 5. Orchestrator Agent Implementation

### 5.1 Overview

The Orchestrator Agent is the central coordinator that:
- Receives all user requests
- Classifies intent to determine which specialist agent should handle it
- Routes requests to appropriate agents
- Manages multi-turn conversations across agents
- Synthesizes responses when multiple agents are involved

### 5.2 Implementation

```csharp
// File: src/ProjectPilot.Agents/Agents/OrchestratorAgent.cs

using ProjectPilot.Agents.Contracts;
using ProjectPilot.LLM;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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
        : base(llmProvider, logger)
    {
        _agentRegistry = agentRegistry;
        _communicationService = communicationService;
    }

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
        var classificationPrompt = $"""
            Classify the following user request into one or more categories.

            User request: {message.Content}

            Categories:
            - planning: Task breakdown, prioritization, estimation, scheduling
            - research: Information lookup, documentation, best practices
            - reporting: Status updates, metrics, progress summaries
            - general: General questions, clarifications, greetings

            Respond in JSON format:
            {{
                "primaryIntent": "planning|research|reporting|general",
                "secondaryIntents": ["intent1", "intent2"],
                "confidence": 0.0-1.0,
                "reasoning": "brief explanation"
            }}
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
            Usage = synthesized.Usage
        };
    }

    private async Task<AgentResponse> HandleGeneralRequestAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken)
    {
        var response = await _llmProvider.ChatAsync(new ChatRequest
        {
            Messages = BuildConversationMessages(context, message)
        }, cancellationToken);

        return new AgentResponse
        {
            Content = response.Content,
            FromAgent = Name,
            Usage = response.Usage
        };
    }

    private List<ChatMessage> BuildConversationMessages(AgentContext context, AgentMessage message)
    {
        var messages = new List<ChatMessage>
        {
            new() { Role = "system", Content = SystemPrompt }
        };

        // Add conversation history
        foreach (var historyMessage in context.ConversationHistory.TakeLast(10))
        {
            messages.Add(new ChatMessage
            {
                Role = historyMessage.Role == MessageRole.User ? "user" : "assistant",
                Content = historyMessage.Content
            });
        }

        // Add current message
        messages.Add(new ChatMessage { Role = "user", Content = message.Content });

        return messages;
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
                Messages = BuildConversationMessages(context, message)
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
```

---

## 6. Planning Agent Implementation

### 6.1 Overview

The Planning Agent specializes in:
- Breaking down high-level goals into actionable tasks
- Prioritizing tasks based on importance and urgency
- Identifying task dependencies
- Estimating effort/complexity
- Creating milestone structures

### 6.2 Implementation

```csharp
// File: src/ProjectPilot.Agents/Agents/PlanningAgent.cs

using ProjectPilot.Agents.Contracts;
using ProjectPilot.LLM;
using Microsoft.Extensions.Logging;

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
        : base(llmProvider, logger)
    {
        _memoryStore = memoryStore;
    }

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
            Usage = response.Usage,
            DurationMs = stopwatch.ElapsedMilliseconds
        };
    }

    private async Task<List<MemoryEntry>> RetrieveProjectContextAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(context.ProjectId))
            return new List<MemoryEntry>();

        return await _memoryStore.SearchAsync(
            query: "project goals tasks milestones",
            projectId: context.ProjectId,
            limit: 5,
            cancellationToken: cancellationToken);
    }

    private string BuildPlanningPrompt(
        AgentMessage message,
        AgentContext context,
        List<MemoryEntry> projectMemory)
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
        var memoryEntry = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = context.SessionId,
            ProjectId = context.ProjectId,
            Type = MemoryType.Plan,
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
```

---

## 7. Research Agent Implementation

### 7.1 Overview

The Research Agent specializes in:
- Web searching for information
- Documentation lookup
- Best practice recommendations
- Technology comparisons
- Source citation

### 7.2 Implementation

```csharp
// File: src/ProjectPilot.Agents/Agents/ResearchAgent.cs

using ProjectPilot.Agents.Contracts;
using ProjectPilot.Agents.Tools;
using ProjectPilot.LLM;
using Microsoft.Extensions.Logging;

namespace ProjectPilot.Agents.Agents;

public class ResearchAgent : BaseAgent
{
    private const string SystemPrompt = """
        You are the Research Agent for ProjectPilot, specializing in finding and synthesizing information.

        Your capabilities:
        1. **Web Search**: Find relevant information from the internet
        2. **Documentation Lookup**: Retrieve technical documentation
        3. **Best Practices**: Provide industry best practice recommendations
        4. **Technology Comparison**: Compare tools, frameworks, and approaches
        5. **Source Citation**: Always cite your sources

        Guidelines:
        - Always cite sources with URLs when available
        - Distinguish between facts and opinions
        - Provide balanced perspectives when comparing options
        - Summarize findings clearly and concisely
        - Highlight key takeaways

        You have access to the following tools:
        - web_search: Search the internet for information
        - fetch_url: Retrieve content from a specific URL

        When researching:
        1. Break down the research question
        2. Search for relevant information
        3. Synthesize findings
        4. Cite all sources

        Format research results with clear sections and bullet points.
        """;

    public override string Name => "Research";
    public override AgentType AgentType => AgentType.Research;
    public override string Description => "Specializes in web search, documentation, and best practice research";

    private readonly IWebSearchTool _webSearchTool;
    private readonly IMemoryStore _memoryStore;

    public ResearchAgent(
        ILLMProvider llmProvider,
        IWebSearchTool webSearchTool,
        IMemoryStore memoryStore,
        ILogger<ResearchAgent> logger)
        : base(llmProvider, logger)
    {
        _webSearchTool = webSearchTool;
        _memoryStore = memoryStore;
    }

    public override async Task<AgentResponse> ProcessAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var toolCalls = new List<ToolCallResult>();

        _logger.LogInformation(
            "Research Agent processing request for session {SessionId}",
            context.SessionId);

        // Step 1: Determine if we need to search
        var searchQueries = await ExtractSearchQueriesAsync(message, cancellationToken);

        // Step 2: Execute searches
        var searchResults = new List<SearchResult>();
        foreach (var query in searchQueries)
        {
            var searchStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var results = await _webSearchTool.SearchAsync(query, cancellationToken);
            searchStopwatch.Stop();

            searchResults.AddRange(results);

            toolCalls.Add(new ToolCallResult
            {
                ToolName = "web_search",
                Parameters = $"{{\"query\": \"{query}\"}}",
                Result = new ToolResult
                {
                    Success = true,
                    Output = $"Found {results.Count} results"
                },
                DurationMs = searchStopwatch.ElapsedMilliseconds
            });
        }

        // Step 3: Synthesize findings
        var synthesisPrompt = BuildSynthesisPrompt(message, searchResults);

        var response = await _llmProvider.ChatAsync(new ChatRequest
        {
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = SystemPrompt },
                new() { Role = "user", Content = synthesisPrompt }
            },
            Temperature = 0.5f,
            MaxTokens = 2000
        }, cancellationToken);

        // Store research in memory
        await StoreResearchInMemoryAsync(context, message.Content, response.Content, cancellationToken);

        stopwatch.Stop();

        return new AgentResponse
        {
            Content = response.Content,
            FromAgent = Name,
            ToolCalls = toolCalls,
            Usage = response.Usage,
            DurationMs = stopwatch.ElapsedMilliseconds
        };
    }

    private async Task<List<string>> ExtractSearchQueriesAsync(
        AgentMessage message,
        CancellationToken cancellationToken)
    {
        var extractionPrompt = $"""
            Extract search queries from the following request.
            Return 1-3 focused search queries that would help answer the question.

            Request: {message.Content}

            Return as JSON array: ["query1", "query2", "query3"]
            """;

        var response = await _llmProvider.ChatAsync(new ChatRequest
        {
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = extractionPrompt }
            },
            Temperature = 0.3f,
            MaxTokens = 200
        }, cancellationToken);

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(response.Content)
                ?? new List<string> { message.Content };
        }
        catch
        {
            return new List<string> { message.Content };
        }
    }

    private string BuildSynthesisPrompt(AgentMessage message, List<SearchResult> searchResults)
    {
        var promptBuilder = new System.Text.StringBuilder();

        promptBuilder.AppendLine("## Research Request");
        promptBuilder.AppendLine(message.Content);
        promptBuilder.AppendLine();

        promptBuilder.AppendLine("## Search Results");
        foreach (var result in searchResults.Take(10))
        {
            promptBuilder.AppendLine($"### {result.Title}");
            promptBuilder.AppendLine($"URL: {result.Url}");
            promptBuilder.AppendLine($"Snippet: {result.Snippet}");
            promptBuilder.AppendLine();
        }

        promptBuilder.AppendLine("## Instructions");
        promptBuilder.AppendLine("Synthesize the search results to answer the research request.");
        promptBuilder.AppendLine("Always cite sources with [Title](URL) format.");
        promptBuilder.AppendLine("Highlight key findings and recommendations.");

        return promptBuilder.ToString();
    }

    private async Task StoreResearchInMemoryAsync(
        AgentContext context,
        string query,
        string findings,
        CancellationToken cancellationToken)
    {
        var memoryEntry = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = context.SessionId,
            ProjectId = context.ProjectId,
            Type = MemoryType.Research,
            Content = $"Research on: {query}\n\nFindings:\n{findings}",
            Timestamp = DateTimeOffset.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["agent"] = Name,
                ["query"] = query
            }
        };

        await _memoryStore.StoreAsync(memoryEntry, cancellationToken);
    }

    public override async IAsyncEnumerable<AgentResponseChunk> ProcessStreamAsync(
        AgentMessage message,
        AgentContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Emit searching status
        yield return new AgentResponseChunk
        {
            Content = "Searching for information...\n\n",
            FromAgent = Name,
            Type = ChunkType.Thinking
        };

        // Execute search (non-streaming)
        var searchQueries = await ExtractSearchQueriesAsync(message, cancellationToken);
        var searchResults = new List<SearchResult>();

        foreach (var query in searchQueries)
        {
            var results = await _webSearchTool.SearchAsync(query, cancellationToken);
            searchResults.AddRange(results);
        }

        yield return new AgentResponseChunk
        {
            Content = $"Found {searchResults.Count} results. Synthesizing...\n\n",
            FromAgent = Name,
            Type = ChunkType.Thinking
        };

        // Stream synthesis
        var synthesisPrompt = BuildSynthesisPrompt(message, searchResults);
        var fullContent = new System.Text.StringBuilder();

        await foreach (var chunk in _llmProvider.ChatStreamAsync(new ChatRequest
        {
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = SystemPrompt },
                new() { Role = "user", Content = synthesisPrompt }
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

        // Store research
        await StoreResearchInMemoryAsync(context, message.Content, fullContent.ToString(), cancellationToken);
    }
}
```

---

## 8. Reporting Agent Implementation

### 8.1 Overview

The Reporting Agent specializes in:
- Generating project status summaries
- Calculating progress metrics
- Identifying blockers and risks
- Creating insights and recommendations

### 8.2 Implementation

```csharp
// File: src/ProjectPilot.Agents/Agents/ReportingAgent.cs

using ProjectPilot.Agents.Contracts;
using ProjectPilot.LLM;
using Microsoft.Extensions.Logging;

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
        : base(llmProvider, logger)
    {
        _memoryStore = memoryStore;
    }

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
            Usage = response.Usage,
            DurationMs = stopwatch.ElapsedMilliseconds
        };
    }

    private async Task<ProjectData> GatherProjectDataAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        var projectData = new ProjectData();

        // Get plans from memory
        var plans = await _memoryStore.SearchAsync(
            query: "tasks milestones plan",
            projectId: context.ProjectId,
            type: MemoryType.Plan,
            limit: 10,
            cancellationToken: cancellationToken);

        projectData.Plans = plans;

        // Get previous reports
        var previousReports = await _memoryStore.SearchAsync(
            query: "status report progress",
            projectId: context.ProjectId,
            type: MemoryType.Report,
            limit: 3,
            cancellationToken: cancellationToken);

        projectData.PreviousReports = previousReports;

        // Get research findings
        var research = await _memoryStore.SearchAsync(
            query: "research findings",
            projectId: context.ProjectId,
            type: MemoryType.Research,
            limit: 5,
            cancellationToken: cancellationToken);

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
        var memoryEntry = new MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = context.SessionId,
            ProjectId = context.ProjectId,
            Type = MemoryType.Report,
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
    public List<MemoryEntry> Plans { get; set; } = new();
    public List<MemoryEntry> PreviousReports { get; set; } = new();
    public List<MemoryEntry> Research { get; set; } = new();
}
```

---

## 9. Agent Communication Protocol

### 9.1 Communication Service

```csharp
// File: src/ProjectPilot.Agents/Services/AgentCommunicationService.cs

using ProjectPilot.Agents.Contracts;
using Microsoft.Extensions.Logging;

namespace ProjectPilot.Agents.Services;

public interface IAgentCommunicationService
{
    /// <summary>
    /// Send a message from one agent to another
    /// </summary>
    Task<AgentResponse> SendMessageAsync(
        string fromAgent,
        string toAgent,
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcast a message to all agents
    /// </summary>
    Task BroadcastAsync(
        string fromAgent,
        AgentMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log an agent handoff
    /// </summary>
    Task LogHandoffAsync(
        string fromAgent,
        string toAgent,
        AgentContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get communication history for a session
    /// </summary>
    Task<List<AgentCommunication>> GetHistoryAsync(
        string sessionId,
        CancellationToken cancellationToken = default);
}

public class AgentCommunicationService : IAgentCommunicationService
{
    private readonly IAgentRegistry _agentRegistry;
    private readonly IAgentActivityLogger _activityLogger;
    private readonly ILogger<AgentCommunicationService> _logger;

    public AgentCommunicationService(
        IAgentRegistry agentRegistry,
        IAgentActivityLogger activityLogger,
        ILogger<AgentCommunicationService> logger)
    {
        _agentRegistry = agentRegistry;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<AgentResponse> SendMessageAsync(
        string fromAgent,
        string toAgent,
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Agent {FromAgent} sending message to {ToAgent}",
            fromAgent, toAgent);

        var targetAgent = _agentRegistry.GetAgentByName(toAgent);
        if (targetAgent == null)
        {
            throw new InvalidOperationException($"Agent '{toAgent}' not found");
        }

        // Log the communication
        await _activityLogger.LogCommunicationAsync(new AgentCommunication
        {
            FromAgent = fromAgent,
            ToAgent = toAgent,
            Message = message,
            SessionId = context.SessionId,
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken);

        // Process with target agent
        return await targetAgent.ProcessAsync(message, context, cancellationToken);
    }

    public async Task BroadcastAsync(
        string fromAgent,
        AgentMessage message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Agent {FromAgent} broadcasting message to all agents",
            fromAgent);

        var agents = _agentRegistry.GetAllAgents()
            .Where(a => a.Name != fromAgent);

        foreach (var agent in agents)
        {
            await _activityLogger.LogCommunicationAsync(new AgentCommunication
            {
                FromAgent = fromAgent,
                ToAgent = agent.Name,
                Message = message,
                Type = CommunicationType.Broadcast,
                Timestamp = DateTimeOffset.UtcNow
            }, cancellationToken);
        }
    }

    public async Task LogHandoffAsync(
        string fromAgent,
        string toAgent,
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        await _activityLogger.LogHandoffAsync(new AgentHandoff
        {
            FromAgent = fromAgent,
            ToAgent = toAgent,
            SessionId = context.SessionId,
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken);
    }

    public Task<List<AgentCommunication>> GetHistoryAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        return _activityLogger.GetCommunicationHistoryAsync(sessionId, cancellationToken);
    }
}

public record AgentCommunication
{
    public string FromAgent { get; init; } = string.Empty;
    public string ToAgent { get; init; } = string.Empty;
    public AgentMessage? Message { get; init; }
    public string? SessionId { get; init; }
    public CommunicationType Type { get; init; } = CommunicationType.Direct;
    public DateTimeOffset Timestamp { get; init; }
}

public enum CommunicationType
{
    Direct,
    Broadcast,
    Handoff
}

public record AgentHandoff
{
    public string FromAgent { get; init; } = string.Empty;
    public string ToAgent { get; init; } = string.Empty;
    public string? SessionId { get; init; }
    public string? Reason { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
```

---

## 10. Agent Handoff Mechanism

### 10.1 Handoff Manager

```csharp
// File: src/ProjectPilot.Agents/Services/AgentHandoffManager.cs

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
```

---

## 11. Parallel Agent Execution

### 11.1 Parallel Execution Service

```csharp
// File: src/ProjectPilot.Agents/Services/ParallelAgentExecutor.cs

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
```

---

## 12. State Management

### 12.1 Agent State Manager

```csharp
// File: src/ProjectPilot.Agents/Services/AgentStateManager.cs

namespace ProjectPilot.Agents.Services;

public interface IAgentStateManager
{
    Task<AgentState?> GetStateAsync(string sessionId, string agentName, CancellationToken ct = default);
    Task SaveStateAsync(AgentState state, CancellationToken ct = default);
    Task ClearStateAsync(string sessionId, CancellationToken ct = default);
}

public record AgentState
{
    public required string SessionId { get; init; }
    public required string AgentName { get; init; }
    public Dictionary<string, object> Data { get; init; } = new();
    public DateTimeOffset LastUpdated { get; init; } = DateTimeOffset.UtcNow;
}

public class AgentStateManager : IAgentStateManager
{
    private readonly IMemoryStore _memoryStore;
    private readonly ILogger<AgentStateManager> _logger;

    public AgentStateManager(
        IMemoryStore memoryStore,
        ILogger<AgentStateManager> logger)
    {
        _memoryStore = memoryStore;
        _logger = logger;
    }

    public async Task<AgentState?> GetStateAsync(
        string sessionId,
        string agentName,
        CancellationToken ct = default)
    {
        var key = BuildStateKey(sessionId, agentName);
        var entry = await _memoryStore.GetAsync(key, ct);

        if (entry == null) return null;

        return System.Text.Json.JsonSerializer.Deserialize<AgentState>(entry.Content);
    }

    public async Task SaveStateAsync(AgentState state, CancellationToken ct = default)
    {
        var key = BuildStateKey(state.SessionId, state.AgentName);

        var entry = new MemoryEntry
        {
            Id = key,
            SessionId = state.SessionId,
            Type = MemoryType.AgentState,
            Content = System.Text.Json.JsonSerializer.Serialize(state),
            Timestamp = DateTimeOffset.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["agent"] = state.AgentName
            }
        };

        await _memoryStore.StoreAsync(entry, ct);

        _logger.LogDebug(
            "Saved state for agent {Agent} in session {Session}",
            state.AgentName, state.SessionId);
    }

    public async Task ClearStateAsync(string sessionId, CancellationToken ct = default)
    {
        await _memoryStore.DeleteBySessionAsync(sessionId, MemoryType.AgentState, ct);

        _logger.LogInformation("Cleared agent states for session {Session}", sessionId);
    }

    private string BuildStateKey(string sessionId, string agentName)
    {
        return $"state:{sessionId}:{agentName}";
    }
}
```

---

## 13. Activity Logging

### 13.1 Activity Logger

```csharp
// File: src/ProjectPilot.Agents/Services/AgentActivityLogger.cs

namespace ProjectPilot.Agents.Services;

public interface IAgentActivityLogger
{
    Task LogActivityAsync(AgentActivity activity, CancellationToken ct = default);
    Task LogCommunicationAsync(AgentCommunication communication, CancellationToken ct = default);
    Task LogHandoffAsync(AgentHandoff handoff, CancellationToken ct = default);
    Task<List<AgentActivity>> GetActivitiesAsync(string sessionId, CancellationToken ct = default);
    Task<List<AgentCommunication>> GetCommunicationHistoryAsync(string sessionId, CancellationToken ct = default);
}

public record AgentActivity
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public required string SessionId { get; init; }
    public required string AgentName { get; init; }
    public required AgentActivityType Type { get; init; }
    public required string Description { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public long? DurationMs { get; init; }
}

public enum AgentActivityType
{
    MessageReceived,
    MessageSent,
    ToolInvoked,
    Handoff,
    StateChanged,
    Error
}

public class AgentActivityLogger : IAgentActivityLogger
{
    private readonly IMemoryStore _memoryStore;
    private readonly ILogger<AgentActivityLogger> _logger;

    public AgentActivityLogger(
        IMemoryStore memoryStore,
        ILogger<AgentActivityLogger> logger)
    {
        _memoryStore = memoryStore;
        _logger = logger;
    }

    public async Task LogActivityAsync(AgentActivity activity, CancellationToken ct = default)
    {
        var entry = new MemoryEntry
        {
            Id = activity.Id,
            SessionId = activity.SessionId,
            Type = MemoryType.Activity,
            Content = System.Text.Json.JsonSerializer.Serialize(activity),
            Timestamp = activity.Timestamp,
            Metadata = new Dictionary<string, object>
            {
                ["agent"] = activity.AgentName,
                ["activity_type"] = activity.Type.ToString()
            }
        };

        await _memoryStore.StoreAsync(entry, ct);

        _logger.LogDebug(
            "Agent {Agent} activity: {Type} - {Description}",
            activity.AgentName, activity.Type, activity.Description);
    }

    public async Task LogCommunicationAsync(AgentCommunication communication, CancellationToken ct = default)
    {
        var activity = new AgentActivity
        {
            SessionId = communication.SessionId ?? "unknown",
            AgentName = communication.FromAgent,
            Type = AgentActivityType.MessageSent,
            Description = $"Sent message to {communication.ToAgent}",
            Metadata = new Dictionary<string, object>
            {
                ["to_agent"] = communication.ToAgent,
                ["communication_type"] = communication.Type.ToString()
            },
            Timestamp = communication.Timestamp
        };

        await LogActivityAsync(activity, ct);
    }

    public async Task LogHandoffAsync(AgentHandoff handoff, CancellationToken ct = default)
    {
        var activity = new AgentActivity
        {
            SessionId = handoff.SessionId ?? "unknown",
            AgentName = handoff.FromAgent,
            Type = AgentActivityType.Handoff,
            Description = $"Handed off to {handoff.ToAgent}",
            Metadata = new Dictionary<string, object>
            {
                ["to_agent"] = handoff.ToAgent,
                ["reason"] = handoff.Reason ?? "unspecified"
            },
            Timestamp = handoff.Timestamp
        };

        await LogActivityAsync(activity, ct);
    }

    public async Task<List<AgentActivity>> GetActivitiesAsync(string sessionId, CancellationToken ct = default)
    {
        var entries = await _memoryStore.GetBySessionAsync(sessionId, MemoryType.Activity, ct);

        return entries
            .Select(e => System.Text.Json.JsonSerializer.Deserialize<AgentActivity>(e.Content)!)
            .OrderBy(a => a.Timestamp)
            .ToList();
    }

    public async Task<List<AgentCommunication>> GetCommunicationHistoryAsync(
        string sessionId,
        CancellationToken ct = default)
    {
        var activities = await GetActivitiesAsync(sessionId, ct);

        return activities
            .Where(a => a.Type == AgentActivityType.MessageSent || a.Type == AgentActivityType.Handoff)
            .Select(a => new AgentCommunication
            {
                FromAgent = a.AgentName,
                ToAgent = a.Metadata?["to_agent"]?.ToString() ?? "unknown",
                SessionId = a.SessionId,
                Timestamp = a.Timestamp
            })
            .ToList();
    }
}
```

---

## 14. Configuration

### 14.1 Agent Configuration Options

```csharp
// File: src/ProjectPilot.Agents/Configuration/AgentOptions.cs

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
```

### 14.2 Sample Configuration (appsettings.json)

```json
{
  "Agents": {
    "Orchestrator": {
      "Enabled": true,
      "Temperature": 0.3,
      "MaxTokens": 1000,
      "EnableParallelExecution": true,
      "MaxHandoffDepth": 5
    },
    "Planning": {
      "Enabled": true,
      "Temperature": 0.7,
      "MaxTokens": 2000,
      "EstimationScale": "TShirt"
    },
    "Research": {
      "Enabled": true,
      "Temperature": 0.5,
      "MaxTokens": 2000,
      "MaxSearchResults": 10,
      "EnableWebSearch": true
    },
    "Reporting": {
      "Enabled": true,
      "Temperature": 0.5,
      "MaxTokens": 2000,
      "DefaultFormat": "Markdown"
    },
    "Global": {
      "TimeoutMs": 60000,
      "EnableActivityLogging": true,
      "MaxConversationHistory": 10
    }
  }
}
```

---

## 15. Dependency Injection Setup

### 15.1 Service Registration

```csharp
// File: src/ProjectPilot.Agents/DependencyInjection/AgentServiceCollectionExtensions.cs

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProjectPilot.Agents.Agents;
using ProjectPilot.Agents.Configuration;
using ProjectPilot.Agents.Contracts;
using ProjectPilot.Agents.Services;

namespace ProjectPilot.Agents.DependencyInjection;

public static class AgentServiceCollectionExtensions
{
    public static IServiceCollection AddProjectPilotAgents(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register configuration
        services.Configure<AgentOptions>(
            configuration.GetSection(AgentOptions.SectionName));

        // Register agents
        services.AddSingleton<IAgent, OrchestratorAgent>();
        services.AddSingleton<IAgent, PlanningAgent>();
        services.AddSingleton<IAgent, ResearchAgent>();
        services.AddSingleton<IAgent, ReportingAgent>();

        // Register agent registry
        services.AddSingleton<IAgentRegistry, AgentRegistry>();

        // Register services
        services.AddSingleton<IAgentCommunicationService, AgentCommunicationService>();
        services.AddSingleton<IAgentHandoffManager, AgentHandoffManager>();
        services.AddSingleton<IParallelAgentExecutor, ParallelAgentExecutor>();
        services.AddSingleton<IAgentStateManager, AgentStateManager>();
        services.AddSingleton<IAgentActivityLogger, AgentActivityLogger>();

        // Register orchestration service (main entry point)
        services.AddScoped<IAgentOrchestrationService, AgentOrchestrationService>();

        return services;
    }
}
```

### 15.2 Agent Registry

```csharp
// File: src/ProjectPilot.Agents/Services/AgentRegistry.cs

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
```

---

## 16. Testing Strategy

### 16.1 Unit Test Example

```csharp
// File: tests/ProjectPilot.Agents.Tests/Agents/OrchestratorAgentTests.cs

using Moq;
using Xunit;
using ProjectPilot.Agents.Agents;
using ProjectPilot.Agents.Contracts;
using ProjectPilot.LLM;
using Microsoft.Extensions.Logging;

namespace ProjectPilot.Agents.Tests.Agents;

public class OrchestratorAgentTests
{
    private readonly Mock<ILLMProvider> _mockLlmProvider;
    private readonly Mock<IAgentRegistry> _mockAgentRegistry;
    private readonly Mock<IAgentCommunicationService> _mockCommunicationService;
    private readonly Mock<ILogger<OrchestratorAgent>> _mockLogger;
    private readonly OrchestratorAgent _agent;

    public OrchestratorAgentTests()
    {
        _mockLlmProvider = new Mock<ILLMProvider>();
        _mockAgentRegistry = new Mock<IAgentRegistry>();
        _mockCommunicationService = new Mock<IAgentCommunicationService>();
        _mockLogger = new Mock<ILogger<OrchestratorAgent>>();

        _agent = new OrchestratorAgent(
            _mockLlmProvider.Object,
            _mockAgentRegistry.Object,
            _mockCommunicationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessAsync_PlanningRequest_DelegatesToPlanningAgent()
    {
        // Arrange
        var message = new AgentMessage
        {
            Content = "Break down my project into tasks",
            Role = MessageRole.User
        };

        var context = new AgentContext
        {
            SessionId = "test-session"
        };

        var mockPlanningAgent = new Mock<IAgent>();
        mockPlanningAgent.Setup(a => a.ProcessAsync(It.IsAny<AgentMessage>(), It.IsAny<AgentContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AgentResponse
            {
                Content = "Here's your task breakdown...",
                FromAgent = "Planning"
            });

        _mockAgentRegistry.Setup(r => r.GetAgent(AgentType.Planning))
            .Returns(mockPlanningAgent.Object);

        _mockLlmProvider.Setup(p => p.ChatAsync(It.IsAny<ChatRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse
            {
                Content = "{\"primaryIntent\": \"planning\", \"confidence\": 0.9}"
            });

        // Act
        var response = await _agent.ProcessAsync(message, context);

        // Assert
        Assert.Equal("Planning", response.FromAgent);
        mockPlanningAgent.Verify(a => a.ProcessAsync(
            It.IsAny<AgentMessage>(),
            It.IsAny<AgentContext>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Break down my project", "planning")]
    [InlineData("Search for best practices", "research")]
    [InlineData("Give me a status report", "reporting")]
    public async Task ProcessAsync_CorrectlyClassifiesIntents(string input, string expectedIntent)
    {
        // This would be an integration test with real LLM
        // For unit tests, we mock the classification
    }
}
```

### 16.2 Integration Test Example

```csharp
// File: tests/ProjectPilot.Agents.Tests/Integration/AgentIntegrationTests.cs

using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ProjectPilot.Agents.Tests.Integration;

public class AgentIntegrationTests : IClassFixture<AgentTestFixture>
{
    private readonly AgentTestFixture _fixture;

    public AgentIntegrationTests(AgentTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task FullConversation_MultipleAgents_WorksCorrectly()
    {
        // Arrange
        var orchestration = _fixture.Services.GetRequiredService<IAgentOrchestrationService>();
        var sessionId = Guid.NewGuid().ToString();

        // Act - Planning request
        var planResponse = await orchestration.ProcessAsync(
            "Break down building a REST API into tasks",
            sessionId);

        // Assert
        Assert.Contains("task", planResponse.Content.ToLower());

        // Act - Research request
        var researchResponse = await orchestration.ProcessAsync(
            "What are best practices for REST API design?",
            sessionId);

        // Assert
        Assert.Contains("http", researchResponse.Content.ToLower());

        // Act - Report request
        var reportResponse = await orchestration.ProcessAsync(
            "Give me a summary of what we discussed",
            sessionId);

        // Assert
        Assert.NotEmpty(reportResponse.Content);
    }
}
```

---

## 17. Error Handling

### 17.1 Agent Exception Types

```csharp
// File: src/ProjectPilot.Agents/Exceptions/AgentExceptions.cs

namespace ProjectPilot.Agents.Exceptions;

public class AgentException : Exception
{
    public string AgentName { get; }
    public string? SessionId { get; }

    public AgentException(string agentName, string message, string? sessionId = null)
        : base(message)
    {
        AgentName = agentName;
        SessionId = sessionId;
    }

    public AgentException(string agentName, string message, Exception innerException, string? sessionId = null)
        : base(message, innerException)
    {
        AgentName = agentName;
        SessionId = sessionId;
    }
}

public class AgentTimeoutException : AgentException
{
    public TimeSpan Timeout { get; }

    public AgentTimeoutException(string agentName, TimeSpan timeout, string? sessionId = null)
        : base(agentName, $"Agent '{agentName}' timed out after {timeout.TotalSeconds}s", sessionId)
    {
        Timeout = timeout;
    }
}

public class AgentHandoffException : AgentException
{
    public string TargetAgent { get; }

    public AgentHandoffException(string agentName, string targetAgent, string message, string? sessionId = null)
        : base(agentName, message, sessionId)
    {
        TargetAgent = targetAgent;
    }
}

public class AgentNotFoundxception : AgentException
{
    public AgentNotFoundxception(string agentName)
        : base(agentName, $"Agent '{agentName}' not found in registry")
    {
    }
}
```

---

## 18. Performance Considerations

### 18.1 Optimization Guidelines

1. **Caching Intent Classification**
   - Cache common intent classifications to avoid repeated LLM calls
   - Use in-memory cache with short TTL for session-specific caching

2. **Streaming Responses**
   - Always prefer streaming for user-facing responses
   - Reduces perceived latency significantly

3. **Parallel Execution**
   - Execute independent agent calls in parallel
   - Use semaphore to limit concurrent LLM calls

4. **Memory Retrieval**
   - Use indexed queries for memory retrieval
   - Limit memory context size to avoid token bloat

5. **Connection Pooling**
   - Reuse HTTP connections for LLM API calls
   - Use IHttpClientFactory for proper connection management

### 18.2 Metrics to Monitor

| Metric | Target | Alert Threshold |
|--------|--------|-----------------|
| Intent Classification Time | < 500ms | > 1000ms |
| Agent Response Time | < 3s | > 5s |
| Handoff Success Rate | > 99% | < 95% |
| Token Usage per Request | < 2000 | > 4000 |
| Parallel Execution Efficiency | > 80% | < 60% |

---

## Appendix A: File Structure

```
src/ProjectPilot.Agents/
├── Agents/
│   ├── BaseAgent.cs
│   ├── OrchestratorAgent.cs
│   ├── PlanningAgent.cs
│   ├── ResearchAgent.cs
│   └── ReportingAgent.cs
├── Configuration/
│   └── AgentOptions.cs
├── Contracts/
│   ├── IAgent.cs
│   ├── IAgentTool.cs
│   ├── AgentContext.cs
│   ├── AgentMessage.cs
│   ├── AgentResponse.cs
│   └── AgentState.cs
├── DependencyInjection/
│   └── AgentServiceCollectionExtensions.cs
├── Exceptions/
│   └── AgentExceptions.cs
├── Services/
│   ├── AgentActivityLogger.cs
│   ├── AgentCommunicationService.cs
│   ├── AgentHandoffManager.cs
│   ├── AgentOrchestrationService.cs
│   ├── AgentRegistry.cs
│   ├── AgentStateManager.cs
│   └── ParallelAgentExecutor.cs
├── Tools/
│   ├── IWebSearchTool.cs
│   └── WebSearchTool.cs
└── ProjectPilot.Agents.csproj
```

---

## Appendix B: References

- [AutoGen.Net GitHub](https://github.com/microsoft/autogen)
- [AutoGen Documentation](https://microsoft.github.io/autogen/)
- [Multi-Agent Systems Design Patterns](https://arxiv.org/abs/2308.08155)
- [Azure OpenAI Best Practices](https://learn.microsoft.com/azure/ai-services/openai/concepts/advanced-prompt-engineering)
