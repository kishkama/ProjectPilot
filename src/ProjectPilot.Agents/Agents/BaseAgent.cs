using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoGen;
using AutoGen.Core;
using Microsoft.Extensions.Logging;
using ProjectPilot.Agents.Contracts;
using ProjectPilot.LLM;

namespace ProjectPilot.Agents.Agents;

/// <summary>
/// Base class for all ProjectPilot agents providing common functionality
/// </summary>
public abstract class BaseAgent : Contracts.IAgent
{
    protected readonly ILLMProvider _llmProvider;
    protected readonly ILogger _logger;
    protected readonly AssistantAgent _autoGenAgent;

    protected BaseAgent(ILLMProvider llmProvider, ILogger logger, string agentName)
    {
        _llmProvider = llmProvider;
        _logger = logger;
        
        // Create AutoGen agent with basic configuration
        _autoGenAgent = new AssistantAgent(
            name: agentName,
            systemMessage: GetSystemPrompt(),
            llmConfig: null); // Will be configured with custom LLM integration
    }

    protected abstract string GetSystemPrompt();

    public abstract string Name { get; }
    public abstract AgentType AgentType { get; }
    public abstract string Description { get; }

    public abstract Task<AgentResponse> ProcessAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default);

    public abstract IAsyncEnumerable<AgentResponseChunk> ProcessStreamAsync(
        AgentMessage message,
        AgentContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Helper method to build conversation messages for LLM calls
    /// </summary>
    protected List<ChatMessage> BuildConversationMessages(
        AgentContext context,
        AgentMessage currentMessage,
        string systemPrompt)
    {
        var messages = new List<ChatMessage>
        {
            new() { Role = "system", Content = systemPrompt }
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
        messages.Add(new ChatMessage
        {
            Role = currentMessage.Role == MessageRole.User ? "user" : "assistant",
            Content = currentMessage.Content
        });

        return messages;
    }
}