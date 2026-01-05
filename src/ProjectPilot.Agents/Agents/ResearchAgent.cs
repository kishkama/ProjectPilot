using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjectPilot.Agents.Contracts;
using ProjectPilot.Agents.Tools;
using ProjectPilot.LLM;

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
        : base(llmProvider, logger, "Research")
    {
        _webSearchTool = webSearchTool;
        _memoryStore = memoryStore;
    }

    protected override string GetSystemPrompt() => SystemPrompt;

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
            Usage = response.Usage != null ? new TokenUsage
            {
                PromptTokens = (int)(response.Usage.InputTokenCount ?? 0),
                CompletionTokens = (int)(response.Usage.OutputTokenCount ?? 0),
                TotalTokens = (int)(response.Usage.TotalTokenCount ?? 0)
            } : null,
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
            return JsonSerializer.Deserialize<List<string>>(response.Content)
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
        var memoryEntry = new ProjectPilot.LLM.MemoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            SessionId = context.SessionId,
            ProjectId = context.ProjectId,
            Type = ProjectPilot.LLM.MemoryType.Research,
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