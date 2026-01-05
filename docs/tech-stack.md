# ProjectPilot - Technology Stack

**Version:** 1.0
**Last Updated:** 18-Dec-2025

---

## Table of Contents

1. [Overview](#1-overview)
2. [Architecture Diagram](#2-architecture-diagram)
3. [Frontend Technologies](#3-frontend-technologies)
4. [Backend Technologies](#4-backend-technologies)
5. [AI/ML Technologies](#5-aiml-technologies)
6. [Data Storage](#6-data-storage)
7. [Cloud Infrastructure](#7-cloud-infrastructure)
8. [DevOps & CI/CD](#8-devops--cicd)
9. [Security](#9-security)
10. [Monitoring & Observability](#10-monitoring--observability)
11. [Development Tools](#11-development-tools)
12. [Package Dependencies](#12-package-dependencies)
13. [Version Requirements](#13-version-requirements)
14. [Technology Decision Matrix](#14-technology-decision-matrix)

---

## 1. Overview

ProjectPilot is built on a modern, cloud-native technology stack optimized for:

- **Cost Efficiency:** Scale-to-zero architecture minimizes idle costs
- **Developer Productivity:** .NET ecosystem with strong tooling
- **AI Integration:** Native support for multiple LLM providers
- **Scalability:** Containerized microservices architecture
- **Maintainability:** Clean architecture with clear separation of concerns

### Stack Summary

| Layer | Technology |
|-------|------------|
| Frontend | Blazor WebAssembly, MudBlazor |
| Backend API | ASP.NET Core 8.0 Minimal APIs |
| Real-time | SignalR |
| Agent Framework | Microsoft AutoGen.Net |
| LLM Providers | Azure OpenAI, Google Gemini |
| Short-term Memory | Azure Cache for Redis |
| Long-term Memory | Azure Cosmos DB |
| Hosting | Azure Container Apps |
| CI/CD | GitHub Actions |
| Monitoring | Azure Monitor, Application Insights |

---

## 2. Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              CLIENT LAYER                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│    ┌────────────────────────────────────────────────────────────────┐       │
│    │                   Blazor WebAssembly                            │       │
│    │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │       │
│    │  │   MudBlazor  │  │   SignalR    │  │    HTTP      │          │       │
│    │  │   Components │  │   Client     │  │   Client     │          │       │
│    │  └──────────────┘  └──────────────┘  └──────────────┘          │       │
│    └────────────────────────────────────────────────────────────────┘       │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      │ HTTPS / WebSocket
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              API LAYER                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│    ┌────────────────────────────────────────────────────────────────┐       │
│    │                  ASP.NET Core 8.0                               │       │
│    │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │       │
│    │  │  Minimal     │  │   SignalR    │  │  Middleware  │          │       │
│    │  │  APIs        │  │   Hub        │  │  Pipeline    │          │       │
│    │  └──────────────┘  └──────────────┘  └──────────────┘          │       │
│    └────────────────────────────────────────────────────────────────┘       │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                            AGENT LAYER                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│    ┌────────────────────────────────────────────────────────────────┐       │
│    │                   AutoGen.Net Framework                         │       │
│    │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌───────────┐ │       │
│    │  │ Orchestrator│ │  Planning   │ │  Research   │ │ Reporting │ │       │
│    │  │   Agent     │ │   Agent     │ │   Agent     │ │   Agent   │ │       │
│    │  └─────────────┘ └─────────────┘ └─────────────┘ └───────────┘ │       │
│    └────────────────────────────────────────────────────────────────┘       │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                    ┌─────────────────┼─────────────────┐
                    ▼                 ▼                 ▼
┌───────────────────────┐ ┌───────────────────┐ ┌───────────────────────────┐
│     LLM LAYER         │ │   MEMORY LAYER    │ │    INFRASTRUCTURE         │
├───────────────────────┤ ├───────────────────┤ ├───────────────────────────┤
│                       │ │                   │ │                           │
│ ┌───────────────────┐ │ │ ┌───────────────┐ │ │ ┌───────────────────────┐ │
│ │   Azure OpenAI    │ │ │ │    Redis      │ │ │ │  Azure Container Apps │ │
│ │   - GPT-4o        │ │ │ │  (Short-term) │ │ │ │  (Scale-to-zero)      │ │
│ │   - GPT-4         │ │ │ └───────────────┘ │ │ └───────────────────────┘ │
│ └───────────────────┘ │ │                   │ │                           │
│                       │ │ ┌───────────────┐ │ │ ┌───────────────────────┐ │
│ ┌───────────────────┐ │ │ │   Cosmos DB   │ │ │ │    Azure Key Vault    │ │
│ │   Google Gemini   │ │ │ │  (Long-term)  │ │ │ │    (Secrets)          │ │
│ │   - Gemini 1.5 Pro│ │ │ └───────────────┘ │ │ └───────────────────────┘ │
│ │   - Gemini Flash  │ │ │                   │ │                           │
│ └───────────────────┘ │ │                   │ │ ┌───────────────────────┐ │
│                       │ │                   │ │ │   Azure Monitor       │ │
│                       │ │                   │ │ │   (Observability)     │ │
│                       │ │                   │ │ └───────────────────────┘ │
└───────────────────────┘ └───────────────────┘ └───────────────────────────┘
```

---

## 3. Frontend Technologies

### 3.1 Blazor WebAssembly

| Attribute | Details |
|-----------|---------|
| **Version** | .NET 8.0 |
| **Type** | Client-side SPA |
| **Purpose** | Primary web interface |

**Why Blazor WebAssembly?**
- Full .NET ecosystem in the browser
- Shared code between client and server
- Strong typing with C#
- No JavaScript framework dependency
- Excellent for scale-to-zero (static files served from CDN)

**Key Features Used:**
- Component-based architecture
- Dependency injection
- HTTP client for API calls
- WebSocket support for SignalR

### 3.2 MudBlazor

| Attribute | Details |
|-----------|---------|
| **Version** | 6.x |
| **Type** | UI Component Library |
| **License** | MIT |

**Why MudBlazor?**
- Material Design components
- Rich set of pre-built components
- Active community and updates
- Excellent documentation
- Built specifically for Blazor

**Components Used:**
| Component | Purpose |
|-----------|---------|
| `MudLayout` | App shell and navigation |
| `MudDrawer` | Side navigation panel |
| `MudAppBar` | Top navigation bar |
| `MudTextField` | Chat input |
| `MudPaper` | Message containers |
| `MudSelect` | Provider selection |
| `MudSwitch` | Theme toggle |
| `MudProgress` | Loading indicators |
| `MudSnackbar` | Notifications |

### 3.3 SignalR Client

| Attribute | Details |
|-----------|---------|
| **Version** | 8.x |
| **Purpose** | Real-time communication |

**Features:**
- WebSocket-based communication
- Automatic reconnection
- Streaming support for LLM responses
- Connection state management

---

## 4. Backend Technologies

### 4.1 ASP.NET Core

| Attribute | Details |
|-----------|---------|
| **Version** | 8.0 LTS |
| **Type** | Web API Framework |
| **Pattern** | Minimal APIs |

**Why ASP.NET Core 8.0?**
- Long-term support (LTS)
- Native AOT compilation support
- Improved performance
- Minimal APIs for cleaner code
- Built-in dependency injection

**Minimal APIs Endpoints:**
```csharp
// Example endpoint structure
app.MapPost("/api/chat", ChatHandler.Handle);
app.MapGet("/api/chat/stream", ChatHandler.HandleStream);
app.MapGet("/api/health", HealthHandler.Handle);
app.MapGet("/api/config", ConfigHandler.Handle);
```

### 4.2 SignalR Server

| Attribute | Details |
|-----------|---------|
| **Version** | 8.x |
| **Purpose** | Real-time hub |

**Hub Methods:**
| Method | Purpose |
|--------|---------|
| `SendMessage` | Send chat message to agents |
| `StreamResponse` | Stream agent responses |
| `JoinSession` | Join a chat session |
| `LeaveSession` | Leave a chat session |

### 4.3 Core Libraries

| Library | Purpose | Version |
|---------|---------|---------|
| `System.Text.Json` | JSON serialization | Built-in |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | Redis caching | 8.x |
| `Microsoft.Azure.Cosmos` | Cosmos DB client | 3.x |
| `Azure.Identity` | Azure authentication | 1.x |
| `Azure.Security.KeyVault.Secrets` | Key Vault access | 4.x |

---

## 5. AI/ML Technologies

### 5.1 Microsoft AutoGen.Net

| Attribute | Details |
|-----------|---------|
| **Version** | 0.2.x |
| **Purpose** | Multi-agent orchestration |
| **License** | MIT |

**Why AutoGen.Net?**
- Official Microsoft framework
- Native .NET support
- Proven multi-agent patterns
- Active development
- Extensible architecture

**Core Concepts:**
| Concept | Description |
|---------|-------------|
| `IAgent` | Base agent interface |
| `ConversableAgent` | Agent with chat capabilities |
| `GroupChat` | Multi-agent conversation |
| `UserProxy` | Human-in-the-loop support |

### 5.2 Azure OpenAI

| Attribute | Details |
|-----------|---------|
| **Service** | Azure OpenAI Service |
| **Models** | GPT-4o, GPT-4, GPT-3.5-Turbo |
| **SDK** | Azure.AI.OpenAI |

**Capabilities:**
- Chat completions
- Function/tool calling
- Streaming responses
- Token management
- Content filtering

**Configuration:**
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://<instance>.openai.azure.com/",
    "DeploymentName": "gpt-4o",
    "ApiVersion": "2024-02-15-preview"
  }
}
```

### 5.3 Google Gemini

| Attribute | Details |
|-----------|---------|
| **Service** | Google AI Studio / Vertex AI |
| **Models** | Gemini 1.5 Pro, Gemini 1.5 Flash |
| **SDK** | REST API (custom client) |

**Capabilities:**
- Chat completions
- Function calling
- Streaming responses
- Multimodal support (future)
- Generous free tier

**Configuration:**
```json
{
  "Gemini": {
    "ApiKey": "{{from-keyvault}}",
    "Model": "gemini-1.5-pro",
    "BaseUrl": "https://generativelanguage.googleapis.com/v1"
  }
}
```

### 5.4 LLM Comparison

| Feature | Azure OpenAI | Google Gemini |
|---------|--------------|---------------|
| **Best For** | Production, Enterprise | Development, Testing |
| **Free Tier** | No | Yes (generous) |
| **SLA** | 99.9% | Best effort |
| **Compliance** | SOC2, HIPAA, etc. | Limited |
| **Latency** | Low | Low-Medium |
| **Context Window** | 128K (GPT-4o) | 1M (Gemini 1.5) |
| **Tool Calling** | Excellent | Good |

---

## 6. Data Storage

### 6.1 Azure Cache for Redis

| Attribute | Details |
|-----------|---------|
| **Service** | Azure Cache for Redis |
| **Tier** | Basic (dev) / Standard (prod) |
| **Purpose** | Short-term memory, caching |

**Use Cases:**
| Use Case | TTL | Data Type |
|----------|-----|-----------|
| Session context | 30 min | JSON |
| Conversation history | 1 hour | List |
| Agent state | 15 min | Hash |
| Response cache | 5 min | String |

**Connection:**
```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration["Redis:ConnectionString"];
    options.InstanceName = "ProjectPilot:";
});
```

### 6.2 Azure Cosmos DB

| Attribute | Details |
|-----------|---------|
| **Service** | Azure Cosmos DB |
| **API** | NoSQL (Core) |
| **Tier** | Serverless |
| **Purpose** | Long-term memory, persistence |

**Why Serverless Tier?**
- Scale-to-zero billing
- No minimum throughput cost
- Pay per request
- Automatic scaling

**Data Model:**

```
Database: ProjectPilot
├── Container: memories
│   ├── Partition Key: /projectId
│   └── Documents: MemoryEntry
├── Container: conversations
│   ├── Partition Key: /sessionId
│   └── Documents: ConversationHistory
├── Container: projects
│   ├── Partition Key: /userId
│   └── Documents: Project
└── Container: activities
    ├── Partition Key: /sessionId
    └── Documents: AgentActivity
```

**Sample Document:**
```json
{
  "id": "mem-123",
  "projectId": "proj-456",
  "sessionId": "sess-789",
  "type": "Plan",
  "content": "Task breakdown for API implementation...",
  "embedding": [0.1, 0.2, ...],
  "timestamp": "2025-12-18T10:00:00Z",
  "metadata": {
    "agent": "Planning",
    "tokens": 450
  }
}
```

### 6.3 Storage Comparison

| Feature | Redis | Cosmos DB |
|---------|-------|-----------|
| **Data Type** | Ephemeral | Persistent |
| **Latency** | Sub-ms | ~10ms |
| **Durability** | In-memory | Replicated |
| **Query** | Key-based | SQL-like |
| **Cost Model** | Per hour | Per request |
| **Scale-to-Zero** | No | Yes (serverless) |

---

## 7. Cloud Infrastructure

### 7.1 Azure Container Apps

| Attribute | Details |
|-----------|---------|
| **Service** | Azure Container Apps |
| **Purpose** | Application hosting |
| **Key Feature** | Scale-to-zero |

**Why Azure Container Apps?**
- True scale-to-zero (no cost when idle)
- Built-in ingress and load balancing
- Automatic HTTPS
- Dapr integration (optional)
- Managed Kubernetes underneath

**Configuration:**
```yaml
# Container App configuration
properties:
  configuration:
    ingress:
      external: true
      targetPort: 8080
      transport: http
    secrets:
      - name: azure-openai-key
        keyVaultUrl: https://kv-projectpilot.vault.azure.net/secrets/azure-openai-key
  template:
    containers:
      - name: projectpilot-api
        image: projectpilot.azurecr.io/api:latest
        resources:
          cpu: 0.5
          memory: 1Gi
    scale:
      minReplicas: 0
      maxReplicas: 10
      rules:
        - name: http-rule
          http:
            metadata:
              concurrentRequests: 50
```

### 7.2 Azure Key Vault

| Attribute | Details |
|-----------|---------|
| **Service** | Azure Key Vault |
| **Purpose** | Secrets management |

**Secrets Stored:**
| Secret | Purpose |
|--------|---------|
| `azure-openai-key` | Azure OpenAI API key |
| `gemini-api-key` | Google Gemini API key |
| `redis-connection` | Redis connection string |
| `cosmos-connection` | Cosmos DB connection string |

### 7.3 Azure Container Registry

| Attribute | Details |
|-----------|---------|
| **Service** | Azure Container Registry |
| **Tier** | Basic |
| **Purpose** | Docker image storage |

### 7.4 Infrastructure as Code

**Bicep Templates:**
```
infra/
├── main.bicep                 # Main deployment
├── modules/
│   ├── container-apps.bicep   # Container Apps environment
│   ├── container-app.bicep    # Individual app
│   ├── cosmos-db.bicep        # Cosmos DB account
│   ├── redis.bicep            # Redis cache
│   ├── keyvault.bicep         # Key Vault
│   └── acr.bicep              # Container Registry
└── parameters/
    ├── dev.bicepparam         # Development parameters
    └── prod.bicepparam        # Production parameters
```

---

## 8. DevOps & CI/CD

### 8.1 GitHub Actions

| Attribute | Details |
|-----------|---------|
| **Platform** | GitHub Actions |
| **Purpose** | CI/CD automation |

**Workflows:**

| Workflow | Trigger | Actions |
|----------|---------|---------|
| `ci.yml` | Push, PR | Build, Test, Lint |
| `deploy-dev.yml` | Push to `develop` | Deploy to dev |
| `deploy-prod.yml` | Push to `main` | Deploy to prod |
| `infra.yml` | Manual | Deploy infrastructure |

**CI Pipeline:**
```yaml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
```

### 8.2 Docker

**Dockerfile:**
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/ProjectPilot.Api -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProjectPilot.Api.dll"]
```

### 8.3 Branch Strategy

```
main (production)
  │
  └── develop (integration)
        │
        ├── feature/MA-001-orchestrator-agent
        ├── feature/LLM-001-azure-provider
        └── bugfix/fix-memory-leak
```

---

## 9. Security

### 9.1 Authentication & Authorization

| Component | Technology |
|-----------|------------|
| Identity Provider | Azure AD B2C (future) |
| API Authentication | API Keys (v1), JWT (future) |
| Authorization | Role-based (future) |

### 9.2 Security Measures

| Measure | Implementation |
|---------|----------------|
| Secrets Management | Azure Key Vault |
| Transport Security | TLS 1.2+ |
| Input Validation | FluentValidation |
| CORS | Strict origin policy |
| Rate Limiting | ASP.NET Rate Limiting |
| Dependency Scanning | Dependabot, Snyk |

### 9.3 Security Headers

```csharp
app.UseSecurityHeaders(options =>
{
    options.AddContentSecurityPolicy(csp =>
    {
        csp.AddDefaultSrc().Self();
        csp.AddScriptSrc().Self().UnsafeInline();
    });
    options.AddXContentTypeOptionsNoSniff();
    options.AddXFrameOptionsDeny();
    options.AddReferrerPolicyStrictOriginWhenCrossOrigin();
});
```

---

## 10. Monitoring & Observability

### 10.1 Azure Monitor

| Component | Purpose |
|-----------|---------|
| Application Insights | APM, traces, logs |
| Log Analytics | Log aggregation |
| Alerts | Proactive notifications |
| Dashboards | Visualization |

### 10.2 Logging

**Structured Logging with Serilog:**
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(
        telemetryConfiguration,
        TelemetryConverter.Traces)
    .Enrich.FromLogContext()
    .CreateLogger();
```

### 10.3 Metrics

| Metric | Type | Purpose |
|--------|------|---------|
| `agent_request_duration` | Histogram | Agent response time |
| `llm_tokens_used` | Counter | Token consumption |
| `active_sessions` | Gauge | Current sessions |
| `handoff_count` | Counter | Agent handoffs |
| `error_count` | Counter | Error tracking |

### 10.4 Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddRedis(configuration["Redis:ConnectionString"])
    .AddCosmosDb(configuration["CosmosDb:ConnectionString"])
    .AddUrlGroup(new Uri(configuration["AzureOpenAI:Endpoint"]), "azure-openai");
```

---

## 11. Development Tools

### 11.1 IDE & Editors

| Tool | Purpose |
|------|---------|
| Visual Studio 2022 | Primary IDE |
| VS Code | Lightweight editing |
| JetBrains Rider | Alternative IDE |

### 11.2 Extensions

**Visual Studio:**
- GitHub Copilot
- ReSharper (optional)
- CodeMaid

**VS Code:**
- C# Dev Kit
- REST Client
- Docker
- Azure Tools

### 11.3 CLI Tools

| Tool | Purpose |
|------|---------|
| `dotnet` | .NET CLI |
| `az` | Azure CLI |
| `docker` | Container management |
| `gh` | GitHub CLI |

### 11.4 Local Development

**Prerequisites:**
```bash
# Required
- .NET 8.0 SDK
- Docker Desktop
- Azure CLI
- Git

# Optional
- Redis (local or Docker)
- Cosmos DB Emulator
```

**Local Setup:**
```bash
# Clone repository
git clone https://github.com/rathinamtrainers/intern_kamal_kishor.git

# Start dependencies
docker-compose up -d redis

# Run application
dotnet run --project src/ProjectPilot.Api

# Run tests
dotnet test
```

---

## 12. Package Dependencies

### 12.1 Core Packages

```xml
<!-- ProjectPilot.Api.csproj -->
<ItemGroup>
  <!-- ASP.NET Core -->
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />

  <!-- SignalR -->
  <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.0" />

  <!-- Azure -->
  <PackageReference Include="Azure.Identity" Version="1.10.4" />
  <PackageReference Include="Azure.Security.KeyVault.Secrets" Version="4.5.0" />
  <PackageReference Include="Microsoft.Azure.Cosmos" Version="3.37.0" />
  <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.0" />

  <!-- Observability -->
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
  <PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.22.0" />
</ItemGroup>
```

### 12.2 Agent Packages

```xml
<!-- ProjectPilot.Agents.csproj -->
<ItemGroup>
  <!-- AutoGen -->
  <PackageReference Include="AutoGen" Version="0.2.0" />
  <PackageReference Include="AutoGen.Core" Version="0.2.0" />

  <!-- Azure OpenAI -->
  <PackageReference Include="Azure.AI.OpenAI" Version="1.0.0-beta.12" />
</ItemGroup>
```

### 12.3 Frontend Packages

```xml
<!-- ProjectPilot.Web.csproj -->
<ItemGroup>
  <!-- Blazor -->
  <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.0" />

  <!-- UI -->
  <PackageReference Include="MudBlazor" Version="6.11.0" />

  <!-- SignalR Client -->
  <PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="8.0.0" />
</ItemGroup>
```

---

## 13. Version Requirements

### 13.1 Runtime Requirements

| Component | Minimum Version | Recommended |
|-----------|-----------------|-------------|
| .NET Runtime | 8.0.0 | 8.0.x (latest) |
| Docker | 20.10 | 24.x |
| Node.js (build only) | 18.x | 20.x |

### 13.2 Azure Service Requirements

| Service | SKU | Notes |
|---------|-----|-------|
| Container Apps | Consumption | Scale-to-zero |
| Cosmos DB | Serverless | Pay-per-request |
| Redis | Basic C0 (dev), Standard C1 (prod) | |
| Key Vault | Standard | |
| Container Registry | Basic | |

### 13.3 Browser Support

| Browser | Minimum Version |
|---------|-----------------|
| Chrome | 90+ |
| Firefox | 90+ |
| Safari | 14+ |
| Edge | 90+ |

---

## 14. Technology Decision Matrix

### 14.1 Frontend Framework Decision

| Criteria | Blazor WASM | React | Angular |
|----------|-------------|-------|---------|
| .NET Integration | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐ |
| Learning Curve | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ |
| Performance | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| Bundle Size | ⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ |
| Code Sharing | ⭐⭐⭐⭐⭐ | ⭐ | ⭐ |
| **Total** | **19** | 14 | 12 |

**Decision:** Blazor WebAssembly - Best .NET integration and code sharing

### 14.2 Database Decision

| Criteria | Cosmos DB | PostgreSQL | MongoDB |
|----------|-----------|------------|---------|
| Scale-to-Zero | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐ |
| Azure Integration | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ |
| Flexible Schema | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ |
| Global Distribution | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ |
| Cost (Low Usage) | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ |
| **Total** | **24** | 12 | 16 |

**Decision:** Azure Cosmos DB - Best scale-to-zero and Azure integration

### 14.3 Hosting Decision

| Criteria | Container Apps | App Service | AKS |
|----------|----------------|-------------|-----|
| Scale-to-Zero | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐ |
| Simplicity | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐ |
| Cost (Low Traffic) | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ |
| Container Native | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Managed Service | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| **Total** | **25** | 17 | 14 |

**Decision:** Azure Container Apps - Best scale-to-zero with container support

---

## Appendix A: Quick Reference

### Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | Yes |
| `AZURE_CLIENT_ID` | Managed identity | Prod only |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | App Insights | Yes |

### API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/chat` | POST | Send chat message |
| `/api/chat/stream` | GET | Stream chat response |
| `/api/health` | GET | Health check |
| `/api/config` | GET | Get configuration |
| `/hub/chat` | WebSocket | SignalR hub |

### Useful Commands

```bash
# Build
dotnet build

# Test
dotnet test

# Run locally
dotnet run --project src/ProjectPilot.Api

# Docker build
docker build -t projectpilot:latest .

# Deploy infrastructure
az deployment group create -g rg-projectpilot -f infra/main.bicep

# Deploy app
az containerapp update -n ca-projectpilot -g rg-projectpilot --image projectpilot.azurecr.io/api:latest
```

---

## Appendix B: References

- [.NET 8 Documentation](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-8)
- [Blazor Documentation](https://learn.microsoft.com/aspnet/core/blazor/)
- [MudBlazor Documentation](https://mudblazor.com/docs/overview)
- [AutoGen Documentation](https://microsoft.github.io/autogen/)
- [Azure Container Apps](https://learn.microsoft.com/azure/container-apps/)
- [Azure Cosmos DB](https://learn.microsoft.com/azure/cosmos-db/)
- [Azure OpenAI Service](https://learn.microsoft.com/azure/ai-services/openai/)
- [Google Gemini API](https://ai.google.dev/docs)
