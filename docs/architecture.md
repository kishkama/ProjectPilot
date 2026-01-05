# ProjectPilot - Architecture Overview

## System Architecture

ProjectPilot is a multi-agent AI assistant platform built with .NET 8, featuring a modular architecture that supports multiple LLM providers and memory systems.

## High-Level Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Web Frontend  │    │   Backend API   │    │   AI Agents     │
│                 │    │                 │    │                 │
│  - Blazor WASM  │◄──►│  - ASP.NET Core │◄──►│  - AutoGen.Net   │
│  - MudBlazor    │    │  - Minimal API  │    │  - Orchestrator │
│  - Real-time UI │    │  - Swagger      │    │  - Planning     │
└─────────────────┘    └─────────────────┘    │  - Research     │
                                             │  - Reporting    │
┌─────────────────┐    ┌─────────────────┐    └─────────────────┘
│   LLM Providers │    │  Memory Systems │
│                 │    │                 │
│  - Azure OpenAI │◄──►│  - Redis Cache  │
│  - Google Gemini│    │  - Cosmos DB    │
└─────────────────┘    └─────────────────┘
```

## Component Overview

### 1. Web Frontend (ProjectPilot.UI)

**Technology**: Blazor WebAssembly + MudBlazor
**Purpose**: User interface for interacting with AI agents

#### Key Features
- **Chat Interface**: Real-time messaging with agents
- **Streaming Support**: Live token-by-token responses
- **Provider Selection**: Choose between Azure OpenAI and Gemini
- **Responsive Design**: Mobile-friendly Material Design UI

#### Architecture
```
Blazor WASM App
├── Pages/
│   ├── Home.razor          # Landing page
│   ├── Chat.razor          # Standard chat
│   └── StreamingChat.razor # Real-time chat
├── Layout/
│   ├── MainLayout.razor    # App shell
│   └── NavMenu.razor       # Navigation
└── Services/
    └── HttpClient          # API communication
```

### 2. Backend API (ProjectPilot.Web)

**Technology**: ASP.NET Core Minimal API
**Purpose**: RESTful API for agent interactions

#### Key Features
- **Chat Endpoints**: Synchronous and streaming chat
- **Health Checks**: System monitoring
- **Swagger Documentation**: Interactive API docs
- **CORS Support**: Cross-origin requests
- **JWT Authentication**: Optional security

#### API Endpoints
```
GET  /health              # Health check
GET  /api/config          # System configuration
POST /api/chat            # Standard chat
POST /api/chat/stream     # Streaming chat
GET  /api/agents          # List agents
GET  /api/sessions/{id}/history  # Session history
```

### 3. Agent System (ProjectPilot.Agents)

**Technology**: AutoGen.Net + Custom Orchestration
**Purpose**: Multi-agent coordination and task execution

#### Agent Types
- **OrchestratorAgent**: Coordinates other agents
- **PlanningAgent**: Creates and manages plans
- **ResearchAgent**: Gathers information and data
- **ReportingAgent**: Generates reports and summaries

#### Architecture
```
Agent System
├── Contracts/
│   ├── IAgent.cs              # Agent interface
│   ├── IAgentRegistry.cs      # Agent discovery
│   └── AgentMessage.cs        # Message types
├── Agents/
│   ├── BaseAgent.cs           # Common functionality
│   ├── OrchestratorAgent.cs   # Main coordinator
│   ├── PlanningAgent.cs       # Task planning
│   ├── ResearchAgent.cs       # Information gathering
│   └── ReportingAgent.cs      # Report generation
├── Services/
│   ├── AgentOrchestrationService.cs
│   ├── AgentCommunicationService.cs
│   └── AgentStateManager.cs
└── Tools/
    └── WebSearchTool.cs        # External tool integration
```

### 4. LLM Abstraction (ProjectPilot.LLM)

**Technology**: Provider pattern with dependency injection
**Purpose**: Unified interface for multiple LLM providers

#### Supported Providers
- **Azure OpenAI**: Enterprise-grade GPT models
- **Google Gemini**: Google's multimodal AI

#### Architecture
```
LLM Layer
├── Contracts/
│   ├── ILLMProvider.cs        # Provider interface
│   └── LLMTypes.cs            # Common types
├── Providers/
│   ├── AzureOpenAIProvider.cs # Azure implementation
│   └── GeminiProvider.cs      # Google implementation
├── Memory/
│   ├── IMemoryStore.cs        # Memory interface
│   ├── RedisMemoryStore.cs    # Short-term memory
│   └── CosmosMemoryStore.cs   # Long-term memory
└── Services/
    ├── LLMServiceCollectionExtensions.cs
    └── ConversationContextManager.cs
```

## Data Flow

### Standard Chat Flow
```
1. User Input → Frontend
2. Frontend → API (/api/chat)
3. API → Orchestration Service
4. Orchestration → Agent Selection
5. Agent → LLM Provider
6. LLM → Response Generation
7. Response → Memory Storage
8. Response → Frontend Display
```

### Streaming Chat Flow
```
1. User Input → Frontend
2. Frontend → API (/api/chat/stream)
3. API → Streaming Orchestration
4. Agent → LLM Provider (streaming)
5. Token Stream → Server-Sent Events
6. Real-time Display → Frontend
7. Complete Response → Memory
```

## Memory Architecture

### Short-term Memory (Redis)
- **Purpose**: Session-based conversation context
- **TTL**: Configurable expiration
- **Use Cases**: Active conversations, recent context

### Long-term Memory (Cosmos DB)
- **Purpose**: Persistent knowledge and history
- **Structure**: Document-based storage
- **Use Cases**: User preferences, learned patterns, historical data

### Memory Management
```csharp
interface IMemoryStore
{
    Task StoreAsync(string key, object data);
    Task<T?> RetrieveAsync<T>(string key);
    Task RemoveAsync(string key);
}

interface IConversationContextManager
{
    Task<ConversationContext> LoadContextAsync(string sessionId);
    Task SaveContextAsync(string sessionId, ConversationContext context);
    Task AddMessageAsync(string sessionId, ChatMessage message);
}
```

## Security Architecture

### Authentication & Authorization
- **JWT Bearer Tokens**: Optional authentication
- **Role-based Access**: Configurable permissions
- **API Keys**: Provider-specific authentication

### Data Protection
- **Encryption**: Data at rest and in transit
- **Key Management**: Azure Key Vault integration
- **Audit Logging**: Comprehensive activity tracking

## Deployment Architecture

### Azure Container Apps
```
Azure Subscription
├── Resource Group
│   ├── Container Apps Environment
│   │   ├── API Container App
│   │   └── UI Container App
│   ├── Azure Container Registry
│   ├── Redis Cache
│   ├── Cosmos DB
│   └── Key Vault
```

### Scaling Strategy
- **Scale-to-Zero**: Cost optimization for low traffic
- **Auto-scaling**: CPU/memory-based scaling rules
- **Horizontal Scaling**: Multiple replicas for high load

## Performance Considerations

### Caching Strategy
- **Redis**: Session data and frequent queries
- **In-memory Cache**: Application-level caching
- **CDN**: Static asset delivery

### Optimization Techniques
- **Async/Await**: Non-blocking operations
- **Connection Pooling**: Database and Redis connections
- **Response Compression**: GZIP compression for API responses

## Monitoring & Observability

### Application Insights
- **Request Tracking**: API performance monitoring
- **Exception Logging**: Error tracking and alerting
- **Custom Metrics**: Agent performance and usage statistics

### Health Checks
- **Liveness Probes**: Container health verification
- **Readiness Probes**: Service availability checks
- **Dependency Checks**: External service connectivity

## Extensibility

### Adding New LLM Providers
```csharp
public class NewProvider : ILLMProvider
{
    public async Task<ChatResponse> ChatAsync(ChatRequest request)
    {
        // Implementation
    }

    public IAsyncEnumerable<StreamChunk> ChatStreamAsync(ChatRequest request)
    {
        // Streaming implementation
    }
}
```

### Adding New Agents
```csharp
public class CustomAgent : BaseAgent
{
    public CustomAgent(ILLMProvider llmProvider, IMemoryStore memory)
        : base("CustomAgent", AgentType.Custom, llmProvider, memory)
    {
    }

    protected override async Task<AgentResponse> ProcessMessageAsync(
        AgentMessage message, AgentContext context)
    {
        // Custom logic
    }
}
```

### Adding New Memory Stores
```csharp
public class CustomMemoryStore : IMemoryStore
{
    public async Task StoreAsync(string key, object data)
    {
        // Custom storage logic
    }

    public async Task<T?> RetrieveAsync<T>(string key)
    {
        // Custom retrieval logic
    }
}
```

## Technology Stack

### Backend
- **Runtime**: .NET 8.0
- **Framework**: ASP.NET Core Minimal API
- **Agent Framework**: AutoGen.Net
- **Database**: Azure Cosmos DB
- **Cache**: Azure Redis Cache

### Frontend
- **Framework**: Blazor WebAssembly
- **UI Library**: MudBlazor
- **Styling**: Material Design
- **State Management**: Blazor component state

### Infrastructure
- **Hosting**: Azure Container Apps
- **Container Registry**: Azure Container Registry
- **CI/CD**: GitHub Actions
- **IaC**: Azure Bicep

### External Services
- **LLM Providers**: Azure OpenAI, Google Gemini
- **Monitoring**: Azure Application Insights
- **Security**: Azure Key Vault

## Development Workflow

### Local Development
1. **Clone Repository**: Get source code
2. **Configure Environment**: Set API keys and connection strings
3. **Build Solution**: `dotnet build`
4. **Run Services**: API and UI in separate terminals
5. **Test Integration**: Use Swagger UI and frontend

### CI/CD Pipeline
1. **Code Changes**: Push to feature branch
2. **Automated Testing**: Unit and integration tests
3. **Build Containers**: Create Docker images
4. **Deploy to Staging**: Automated deployment
5. **Manual Approval**: Promote to production

## Future Enhancements

### Planned Features
- **Voice Interface**: Speech-to-text and text-to-speech
- **File Upload**: Document processing and analysis
- **Multi-language Support**: Internationalization
- **Advanced Analytics**: Usage patterns and insights
- **Plugin System**: Extensible agent capabilities

### Scalability Improvements
- **Microservices**: Break down monolithic components
- **Event-driven Architecture**: Asynchronous processing
- **Global Distribution**: Multi-region deployment
- **Advanced Caching**: Multi-level caching strategy

This architecture provides a solid foundation for a scalable, maintainable, and extensible AI assistant platform.