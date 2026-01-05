# ProjectPilot API Documentation

## Overview

ProjectPilot provides a RESTful API for interacting with multi-agent AI systems. The API supports both standard chat and real-time streaming responses.

## Base URL
```
https://your-app.azurecontainerapps.io
```

## Authentication

Currently, the API does not require authentication for basic usage. JWT authentication can be enabled for production deployments.

## Endpoints

### Health Check

Get the health status of the API.

**Endpoint:** `GET /health`

**Response:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0012345",
  "entries": {
    "self": {
      "data": {},
      "description": "API is healthy",
      "duration": "00:00:00.0001234",
      "status": "Healthy",
      "tags": ["api"]
    }
  }
}
```

### Configuration

Get API configuration and feature flags.

**Endpoint:** `GET /api/config`

**Response:**
```json
{
  "version": "1.0.0",
  "environment": "Production",
  "features": {
    "chat": true,
    "streaming": true,
    "agents": true,
    "memory": true
  }
}
```

### Standard Chat

Send a message and receive a complete response from the AI agents.

**Endpoint:** `POST /api/chat`

**Request Body:**
```json
{
  "message": "Hello, how can you help me today?",
  "sessionId": "optional-session-id"
}
```

**Response:**
```json
{
  "sessionId": "generated-or-provided-session-id",
  "message": "Hello! I'm ProjectPilot, your AI assistant...",
  "usage": {
    "promptTokens": 25,
    "completionTokens": 150,
    "totalTokens": 175
  },
  "agent": "OrchestratorAgent",
  "durationMs": 1250
}
```

**Error Responses:**
- `400 Bad Request`: Invalid request format
- `500 Internal Server Error`: Server error

### Streaming Chat

Send a message and receive real-time streaming responses.

**Endpoint:** `POST /api/chat/stream`

**Request Body:**
```json
{
  "message": "Tell me about artificial intelligence",
  "sessionId": "optional-session-id"
}
```

**Response:** Server-Sent Events stream
```
data: {"content": "Artificial", "agent": "OrchestratorAgent"}
data: {"content": " intelligence", "agent": "OrchestratorAgent"}
data: {"content": " is", "agent": "OrchestratorAgent"}
...
data: [DONE]
```

**Stream Event Format:**
```json
{
  "content": "token text",
  "agent": "AgentName",
  "isComplete": false
}
```

### List Agents

Get information about all available AI agents.

**Endpoint:** `GET /api/agents`

**Response:**
```json
[
  {
    "name": "OrchestratorAgent",
    "agentType": "Orchestrator",
    "description": "Coordinates and manages other agents"
  },
  {
    "name": "PlanningAgent",
    "agentType": "Planning",
    "description": "Creates and manages execution plans"
  },
  {
    "name": "ResearchAgent",
    "agentType": "Research",
    "description": "Gathers information and conducts research"
  },
  {
    "name": "ReportingAgent",
    "agentType": "Reporting",
    "description": "Generates reports and summaries"
  }
]
```

### Session History

Get conversation history for a specific session.

**Endpoint:** `GET /api/sessions/{sessionId}/history`

**Parameters:**
- `sessionId` (path): The session identifier

**Response:**
```json
[
  {
    "role": "user",
    "content": "Hello, how are you?",
    "timestamp": "2024-01-05T10:30:00Z"
  },
  {
    "role": "assistant",
    "content": "I'm doing well, thank you for asking!",
    "timestamp": "2024-01-05T10:30:02Z"
  }
]
```

**Error Responses:**
- `404 Not Found`: Session not found

## Data Types

### ChatRequest
```csharp
public record ChatRequest(string Message, string? SessionId = null);
```

### ChatResponse
```csharp
public record ChatResponse(
    string SessionId,
    string Message,
    UsageDetails? Usage,
    string Agent,
    long DurationMs
);
```

### UsageDetails
```csharp
public record UsageDetails
{
    public int? PromptTokens { get; init; }
    public int? CompletionTokens { get; init; }
    public int? TotalTokens { get; init; }
}
```

### StreamChunk
```csharp
public record StreamChunk
{
    public string Content { get; set; }
    public string Agent { get; set; }
}
```

## Error Handling

All API errors return appropriate HTTP status codes with JSON error details:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "The request is invalid",
  "instance": "/api/chat"
}
```

## Rate Limiting

The API implements rate limiting to prevent abuse:
- **Standard Chat**: 100 requests per minute per IP
- **Streaming Chat**: 50 concurrent streams per IP
- **History**: 1000 requests per hour per IP

Rate limit headers are included in responses:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 99
X-RateLimit-Reset: 1640995200
```

## CORS Policy

The API allows cross-origin requests from configured origins. Default policy allows all origins for development.

## Versioning

The API uses URL versioning. Current version is v1.

## SDKs and Libraries

### JavaScript/TypeScript
```javascript
// Example usage with fetch
const response = await fetch('/api/chat', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    message: 'Hello!',
    sessionId: 'my-session'
  })
});

const result = await response.json();
console.log(result.message);
```

### C#
```csharp
// Using HttpClient
using var client = new HttpClient();
client.BaseAddress = new Uri("https://api.example.com");

var request = new ChatRequest("Hello!", "my-session");
var response = await client.PostAsJsonAsync("/api/chat", request);
var result = await response.Content.ReadFromJsonAsync<ChatResponse>();
```

## Webhooks

The API supports webhooks for real-time notifications (future feature).

## Monitoring

API usage can be monitored through:
- Azure Application Insights
- Health check endpoints
- Request/response logging

## Support

For API support:
- Check the Swagger UI at `/swagger`
- Review this documentation
- Create GitHub issues for bugs or feature requests

## Changelog

### v1.0.0
- Initial release
- Standard and streaming chat endpoints
- Agent listing and session history
- Health checks and configuration endpoints