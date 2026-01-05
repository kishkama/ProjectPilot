# ProjectPilot

AI Learning in C# - A Multi-Agent AI Assistant Platform

## Overview

ProjectPilot is an AI-powered multi-agent system built in C# that demonstrates advanced AI orchestration capabilities. It features a modular architecture supporting multiple LLM providers (Azure OpenAI, Google Gemini) and memory systems (Redis, Cosmos DB).

## Features

- 🤖 **Multi-Agent System**: Orchestrator, Planning, Research, and Reporting agents
- 🧠 **Dual LLM Support**: Azure OpenAI and Google Gemini integration
- 💾 **Memory Systems**: Short-term (Redis) and long-term (Cosmos DB) memory
- 🌐 **Web Interface**: Blazor WebAssembly frontend with real-time chat
- 📡 **Streaming Responses**: Server-Sent Events for live AI responses
- 🏗️ **Modular Architecture**: Extensible provider and agent system
- 📚 **API Documentation**: Interactive Swagger UI

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Visual Studio Code with C# extension
- Git

### Getting Started

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd ProjectPilot
   ```

2. **Open in Visual Studio Code**:
   ```bash
   code .
   ```

3. **Build the solution**:
   ```bash
   dotnet build
   ```

4. **Run the API**:
   ```bash
   dotnet run --project src/ProjectPilot.Web
   ```

5. **Access the application**:
   - **API**: https://localhost:5001
   - **Swagger UI**: https://localhost:5001/swagger
   - **Frontend**: Run `dotnet run --project src/ProjectPilot.UI` for the web interface

## Documentation

- 📖 **[Architecture Overview](docs/architecture.md)** - System design and component relationships
- 🛠️ **[Development Guide](docs/development.md)** - Local setup and development workflow
- 🚀 **[Deployment Guide](docs/deployment.md)** - Azure Container Apps deployment
- 📡 **[API Documentation](docs/api.md)** - REST API reference
- 📋 **[Iteration 1](docs/iteration-1.md)** - MVP feature specifications

## Project Structure

```
ProjectPilot/
├── src/
│   ├── ProjectPilot.Web/          # ASP.NET Core API + Swagger
│   ├── ProjectPilot.UI/           # Blazor WebAssembly frontend
│   ├── ProjectPilot.Agents/       # AutoGen.Net agent implementations
│   └── ProjectPilot.LLM/          # LLM providers & memory systems
├── docs/                          # Documentation
├── infrastructure/                # Azure Bicep templates
├── .github/workflows/             # CI/CD pipelines
└── docker/                        # Container configurations
```

## Technology Stack

### Backend
- **Runtime**: .NET 8.0
- **Framework**: ASP.NET Core Minimal API
- **Agent Framework**: AutoGen.Net
- **UI Framework**: Blazor WebAssembly + MudBlazor

### Infrastructure
- **Hosting**: Azure Container Apps
- **Database**: Azure Cosmos DB
- **Cache**: Azure Redis Cache
- **Registry**: Azure Container Registry

### External Services
- **LLM Providers**: Azure OpenAI, Google Gemini
- **Monitoring**: Azure Application Insights

## Configuration

### Environment Variables
```bash
# Azure OpenAI
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_KEY=your-api-key
AZURE_OPENAI_DEPLOYMENT=gpt-4

# Google Gemini
GOOGLE_GEMINI_API_KEY=your-gemini-api-key

# Memory Systems
REDIS_CONNECTION_STRING=localhost:6379
COSMOS_ENDPOINT=https://your-cosmos.documents.azure.com:443/
COSMOS_KEY=your-cosmos-key
```

## API Examples

### Standard Chat
```bash
curl -X POST https://localhost:5001/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello, how can you help me?"}'
```

### Streaming Chat
```bash
curl -X POST https://localhost:5001/api/chat/stream \
  -H "Content-Type: application/json" \
  -d '{"message": "Tell me about AI"}'
```

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Make changes and test locally
4. Commit with descriptive messages
5. Push and create a pull request

### Development Guidelines
- Follow C# coding standards
- Add unit tests for new features
- Update documentation for API changes
- Use meaningful commit messages

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

- 📧 **Issues**: [Create GitHub Issue](https://github.com/your-org/project-pilot/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/your-org/project-pilot/discussions)
- 📚 **Documentation**: Check the [docs/](docs/) directory

## Roadmap

- ✅ **Iteration 1 (MVP)**: Multi-agent system with web interface
- 🔄 **Iteration 2**: Voice interface and file processing
- 🔄 **Iteration 3**: Plugin system and advanced analytics
- 🔄 **Iteration 4**: Multi-language support and global deployment

---

Built with ❤️ using .NET 8.0 and Azure
