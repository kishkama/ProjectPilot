using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ProjectPilot.Agents.Contracts;
using ProjectPilot.Agents.Services;
using ProjectPilot.Agents.DependencyInjection;
using ProjectPilot.LLM;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add authentication (optional - can be configured for production)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Configure JWT validation - simplified for demo
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false
        };
    });

builder.Services.AddAuthorization();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API is healthy"), tags: new[] { "api" });

// Add ProjectPilot services
builder.Services.AddLLMProviders(builder.Configuration);
// builder.Services.AddProjectPilotAgents(builder.Configuration); // Using mock instead

// Register mock orchestration service
// builder.Services.AddSingleton<IAgentOrchestrationService, MockAgentOrchestrationService>();
// builder.Services.AddSingleton<IAgentRegistry, MockAgentRegistry>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Use CORS
app.UseCors("AllowAll");

// Use authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

// Root endpoint
app.MapGet("/", () => new
{
    message = "Welcome to ProjectPilot API",
    version = "1.0.0",
    description = "Multi-agent AI assistant platform with AutoGen.Net integration",
    endpoints = new
    {
        health = "/health",
        config = "/api/config",
        chat = "/api/chat",
        agents = "/api/agents",
        openapi = "/openapi/v1.json"
    }
})
.WithName("Root")
.WithDescription("API information and available endpoints")
.WithTags("Info");

// Health check endpoint
app.MapHealthChecks("/health")
    .WithName("HealthCheck")
    .WithDescription("Check the health status of the API")
    .WithTags("Health");

// Configuration endpoint
app.MapGet("/api/config", () => new
{
    version = "1.0.0",
    environment = app.Environment.EnvironmentName,
    features = new
    {
        chat = true,
        streaming = true,
        agents = true,
        memory = true
    }
})
.WithName("GetConfiguration")
.WithDescription("Get API configuration and feature flags")
.WithTags("Configuration");

// Test chat endpoint
app.MapPost("/api/chat", (ChatRequest request) =>
{
    return new ChatResponse(
        Guid.NewGuid().ToString(),
        $"Hello! You said: '{request.Message}'. This is a test response from ProjectPilot MVP!",
        new UsageDetails { PromptTokens = 5, CompletionTokens = 15, TotalTokens = 20 },
        "TestAgent",
        50
    );
})
.WithName("Chat")
.WithDescription("Send a message to the AI agents and receive a complete response")
.WithTags("Chat")
.Produces<ChatResponse>()
.ProducesProblem(400)
.ProducesProblem(500);

// Simple test endpoint
app.MapGet("/api/test", () => "API is working!")
.WithName("Test")
.WithDescription("Simple test endpoint")
.WithTags("Test");

// Chat endpoint with streaming
/*
app.MapPost("/api/chat/stream", async (
    ChatRequest request,
    IAgentOrchestrationService orchestrationService,
    IConversationContextManager contextManager,
    ClaimsPrincipal user,
    HttpResponse response,
    CancellationToken cancellationToken) =>
{
    var sessionId = request.SessionId ?? Guid.NewGuid().ToString();
    var userId = user?.Identity?.Name ?? "anonymous";

    response.Headers.ContentType = "text/event-stream";
    response.Headers.CacheControl = "no-cache";
    response.Headers.Connection = "keep-alive";

    // Load conversation context
    var context = await contextManager.LoadContextAsync(sessionId, cancellationToken);

    var agentContext = new AgentContext
    {
        SessionId = sessionId,
        UserId = userId,
        ConversationHistory = context.Messages.Select(m => new AgentMessage
        {
            Id = Guid.NewGuid().ToString(),
            Role = m.Role == "user" ? MessageRole.User : MessageRole.Agent,
            Content = m.Content,
            Timestamp = DateTimeOffset.UtcNow
        }).ToList(),
        ShortTermMemory = context.ShortTermMemory,
        LongTermMemory = context.LongTermMemory
    };

    var agentMessage = new AgentMessage
    {
        Id = Guid.NewGuid().ToString(),
        Role = MessageRole.User,
        Content = request.Message,
        Timestamp = DateTimeOffset.UtcNow
    };

    // Process with streaming
    await foreach (var chunk in orchestrationService.ProcessMessageStreamAsync(agentMessage, agentContext, cancellationToken))
    {
        var eventData = new
        {
            sessionId,
            chunk = chunk.Content,
            isComplete = chunk.IsComplete,
            agent = chunk.FromAgent
        };

        await response.WriteAsync($"data: {JsonSerializer.Serialize(eventData)}\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }

    // Save final message
    await contextManager.AddMessageAsync(sessionId, new ChatMessage
    {
        Role = "user",
        Content = request.Message
    }, cancellationToken);
})
.WithName("ChatStream")
.WithDescription("Send a message and receive streaming responses from AI agents")
.WithTags("Chat", "Streaming")
.Produces(200)
.ProducesProblem(400)
.ProducesProblem(500);
*/

// Agents endpoint
/*
app.MapGet("/api/agents", async (IAgentRegistry agentRegistry) =>
{
    var agents = agentRegistry.GetAllAgents();
    return agents.Select(a => new
    {
        a.Name,
        a.AgentType,
        a.Description
    });
})
.WithName("GetAgents")
.WithDescription("Get information about all available AI agents")
.WithTags("Agents")
.Produces<IEnumerable<object>>();
*/

// Session history endpoint
/*
app.MapGet("/api/sessions/{sessionId}/history", async (
    string sessionId,
    IConversationContextManager contextManager) =>
{
    var messages = await contextManager.GetRecentMessagesAsync(sessionId, 50);
    return messages;
})
.WithName("GetSessionHistory")
.WithDescription("Get conversation history for a specific session")
.WithTags("Sessions")
.Produces<IEnumerable<ChatMessage>>()
.ProducesProblem(404);
*/

app.Run();

// Mock orchestration service for testing
public class MockAgentOrchestrationService : IAgentOrchestrationService
{
    public Task<AgentResponse> ProcessMessageAsync(AgentMessage message, AgentContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AgentResponse
        {
            Content = $"Hello! This is a mock response to your message: '{message.Content}'. The ProjectPilot MVP is working!",
            FromAgent = "MockAgent",
            Usage = new TokenUsage { PromptTokens = 5, CompletionTokens = 10, TotalTokens = 15 },
            DurationMs = 100
        });
    }

    public async IAsyncEnumerable<AgentResponseChunk> ProcessMessageStreamAsync(AgentMessage message, AgentContext context, CancellationToken cancellationToken = default)
    {
        var response = $"Hello! This is a streaming mock response to: '{message.Content}'.";
        
        foreach (var chunk in response)
        {
            yield return new AgentResponseChunk
            {
                Content = chunk.ToString(),
                FromAgent = "MockAgent",
                IsComplete = false
            };
            await Task.Delay(10, cancellationToken);
        }
        
        yield return new AgentResponseChunk
        {
            Content = "",
            FromAgent = "MockAgent",
            IsComplete = true
        };
    }
}

// Mock agent registry for testing
public class MockAgentRegistry : IAgentRegistry
{
    public IAgent? GetAgent(AgentType type) => null;
    public IAgent? GetAgentByName(string name) => null;
    public IEnumerable<IAgent> GetAllAgents() => Array.Empty<IAgent>();
    public void RegisterAgent(IAgent agent) { }
}

// Request/Response models
public record ChatRequest(string Message, string? SessionId = null);
public record ChatResponse(string SessionId, string Message, UsageDetails? Usage, string Agent, long DurationMs);
