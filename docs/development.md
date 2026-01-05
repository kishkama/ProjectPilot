# ProjectPilot - Local Development Guide

## Overview

This guide provides instructions for setting up and running ProjectPilot locally for development and testing.

## Prerequisites

### System Requirements
- **Operating System**: Windows 10/11, macOS 12+, or Linux (Ubuntu 18.04+)
- **.NET SDK**: .NET 8.0 or later
- **Node.js**: Version 16 or later (for frontend development)
- **Git**: Latest version
- **Visual Studio Code**: Recommended IDE with C# and Blazor extensions

### Required Tools
- **Docker Desktop**: For containerized development
- **Azure CLI**: For Azure resource management (optional)
- **GitHub CLI**: For repository operations (optional)

## Quick Start

### 1. Clone the Repository
```bash
git clone https://github.com/your-org/project-pilot.git
cd project-pilot
```

### 2. Build the Solution
```bash
# Build all projects
dotnet build

# Or build specific project
dotnet build src/ProjectPilot.Web
```

### 3. Run the Backend API
```bash
# Navigate to web project
cd src/ProjectPilot.Web

# Run the API server
dotnet run
```

The API will be available at:
- **API Base URL**: `https://localhost:5001` (HTTPS) / `http://localhost:5000` (HTTP)
- **Swagger UI**: `https://localhost:5001/swagger`
- **Health Check**: `https://localhost:5001/health`

### 4. Run the Frontend (Optional)
```bash
# In a separate terminal, navigate to UI project
cd src/ProjectPilot.UI

# Run the Blazor WebAssembly app
dotnet run
```

The frontend will be available at: `https://localhost:5002`

## Configuration

### Environment Variables

Create a `.env` file in the `src/ProjectPilot.Web` directory or set environment variables:

```bash
# Azure OpenAI Configuration
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_KEY=your-api-key
AZURE_OPENAI_DEPLOYMENT=your-deployment-name

# Google Gemini Configuration
GOOGLE_GEMINI_API_KEY=your-gemini-api-key

# Redis Configuration (for production)
REDIS_CONNECTION_STRING=localhost:6379

# Cosmos DB Configuration (for production)
COSMOS_ENDPOINT=https://your-cosmos.documents.azure.com:443/
COSMOS_KEY=your-cosmos-key
COSMOS_DATABASE=projectpilot
```

### appsettings.json

The `appsettings.json` file contains default configuration:

```json
{
  "LLM": {
    "DefaultProvider": "azure-openai",
    "AzureOpenAI": {
      "Endpoint": "https://your-resource.openai.azure.com/",
      "Key": "your-api-key",
      "Deployment": "gpt-4"
    },
    "Gemini": {
      "ApiKey": "your-gemini-api-key",
      "Model": "gemini-pro"
    }
  },
  "Memory": {
    "DefaultStore": "redis",
    "Redis": {
      "ConnectionString": "localhost:6379"
    },
    "CosmosDb": {
      "Endpoint": "https://your-cosmos.documents.azure.com:443/",
      "Key": "your-cosmos-key",
      "DatabaseName": "projectpilot"
    }
  }
}
```

## Development Workflow

### 1. Code Changes
- Make changes to the relevant project files
- Build frequently: `dotnet build`
- Run tests: `dotnet test` (when available)

### 2. API Testing
Use the Swagger UI or tools like Postman/cURL:

```bash
# Health check
curl https://localhost:5001/health

# Send a chat message
curl -X POST https://localhost:5001/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello, how can you help me?", "sessionId": "test-session"}'
```

### 3. Frontend Development
- The Blazor WebAssembly app supports hot reload
- Changes to `.razor` files are reflected immediately
- For component library changes, rebuild may be required

## Project Structure

```
ProjectPilot/
├── src/
│   ├── ProjectPilot.Web/          # ASP.NET Core API
│   ├── ProjectPilot.UI/           # Blazor WebAssembly frontend
│   ├── ProjectPilot.Agents/       # Agent orchestration
│   └── ProjectPilot.LLM/          # LLM providers & memory
├── docs/                          # Documentation
├── infrastructure/                # Azure Bicep templates
├── .github/workflows/             # CI/CD pipelines
└── docker/                        # Container configurations
```

## Debugging

### Backend API
- Use Visual Studio Code debugger or `dotnet watch run`
- Check logs in the console output
- Use Swagger UI for API testing

### Frontend
- Use browser developer tools
- Check browser console for JavaScript errors
- Use Blazor WebAssembly debugging in VS Code

### Common Issues

#### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

#### Port Conflicts
- Change ports in `launchSettings.json` or `Properties/launchSettings.json`
- Default ports: API (5000/5001), UI (5002/5003)

#### CORS Issues
- Ensure the frontend is running on the expected port
- Check CORS policy in `Program.cs`

## Testing

### Unit Tests
```bash
# Run all tests
dotnet test

# Run specific project tests
dotnet test src/ProjectPilot.Agents
```

### Integration Tests
- Use the API endpoints directly
- Test with different LLM providers
- Verify agent handoffs and memory persistence

## Contributing

1. Create a feature branch: `git checkout -b feature/your-feature`
2. Make changes and test locally
3. Commit with descriptive messages
4. Push and create a pull request

## Support

- **Issues**: Create GitHub issues for bugs or feature requests
- **Discussions**: Use GitHub Discussions for questions
- **Documentation**: Check this guide and API documentation

## Next Steps

- Review the [Architecture Overview](architecture.md)
- Check the [Deployment Guide](deployment.md)
- Explore the [API Documentation](api.md)