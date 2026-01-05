# ProjectPilot - Deployment Guide

## Overview

This guide covers deploying ProjectPilot to Azure Container Apps with supporting infrastructure.

## Prerequisites

### Azure Requirements
- **Azure Subscription**: Active subscription with sufficient credits
- **Azure CLI**: Latest version installed and authenticated
- **Resource Group**: Create or use existing resource group

### Required Permissions
- **Contributor** role on the subscription or resource group
- **User Access Administrator** (for managed identity setup)

### Tools
- **Azure CLI**: `az --version`
- **Docker**: For building container images
- **GitHub CLI** (optional): For repository operations

## Quick Deployment

### 1. Clone and Prepare
```bash
git clone https://github.com/your-org/project-pilot.git
cd project-pilot
```

### 2. Set Environment Variables
```bash
# Azure OpenAI
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_KEY="your-api-key"
export AZURE_OPENAI_DEPLOYMENT="gpt-4"

# Google Gemini
export GOOGLE_GEMINI_API_KEY="your-gemini-api-key"

# Azure Resources
export AZURE_SUBSCRIPTION_ID="your-subscription-id"
export AZURE_RESOURCE_GROUP="projectpilot-rg"
export AZURE_LOCATION="eastus"
```

### 3. Deploy Infrastructure
```bash
# Login to Azure
az login

# Set subscription
az account set --subscription $AZURE_SUBSCRIPTION_ID

# Create resource group
az group create --name $AZURE_RESOURCE_GROUP --location $AZURE_LOCATION

# Deploy infrastructure
az deployment group create \
  --resource-group $AZURE_RESOURCE_GROUP \
  --template-file infrastructure/main.bicep \
  --parameters environmentName=prod
```

### 4. Build and Push Container Images
```bash
# Build API image
docker build -t projectpilot-api:latest -f src/ProjectPilot.Web/Dockerfile .

# Build UI image
docker build -t projectpilot-ui:latest -f src/ProjectPilot.UI/Dockerfile .

# Login to Azure Container Registry
az acr login --name yourregistry

# Push images
docker tag projectpilot-api:latest yourregistry.azurecr.io/projectpilot-api:latest
docker tag projectpilot-ui:latest yourregistry.azurecr.io/projectpilot-ui:latest
docker push yourregistry.azurecr.io/projectpilot-api:latest
docker push yourregistry.azurecr.io/projectpilot-ui:latest
```

### 5. Deploy Applications
```bash
# Update container app images
az containerapp update \
  --name projectpilot-api \
  --resource-group $AZURE_RESOURCE_GROUP \
  --image yourregistry.azurecr.io/projectpilot-api:latest

az containerapp update \
  --name projectpilot-ui \
  --resource-group $AZURE_RESOURCE_GROUP \
  --image yourregistry.azurecr.io/projectpilot-ui:latest
```

## Detailed Deployment Steps

### Infrastructure Setup

The infrastructure is defined using Azure Bicep templates:

#### Resources Created
- **Container Apps Environment**: Hosting environment for containerized apps
- **Container Apps**: API and UI applications
- **Azure Container Registry**: Private container registry
- **Redis Cache**: Session storage and caching
- **Cosmos DB**: Long-term memory storage
- **Key Vault**: Secret management
- **Log Analytics**: Monitoring and logging

#### Deployment Parameters
```bicep
param environmentName string = 'dev'
param location string = resourceGroup().location
param apiImage string = 'mcr.microsoft.com/dotnet/aspnet:8.0'
param uiImage string = 'nginx:alpine'
```

### Application Configuration

#### Environment Variables
Set these in the Container Apps configuration:

```bash
# API Configuration
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_KEY=your-api-key
AZURE_OPENAI_DEPLOYMENT=gpt-4
GOOGLE_GEMINI_API_KEY=your-gemini-api-key
REDIS_CONNECTION_STRING=your-redis-connection
COSMOS_ENDPOINT=https://your-cosmos.documents.azure.com:443/
COSMOS_KEY=your-cosmos-key
```

#### Scaling Configuration
```bash
# Set minimum replicas to 0 for cost optimization
az containerapp update \
  --name projectpilot-api \
  --resource-group $AZURE_RESOURCE_GROUP \
  --min-replicas 0 \
  --max-replicas 10
```

### CI/CD Deployment

#### GitHub Actions Setup
The repository includes GitHub Actions workflows for automated deployment:

1. **CI Pipeline** (`.github/workflows/ci.yml`):
   - Builds and tests code
   - Creates container images
   - Pushes to Azure Container Registry

2. **CD Pipeline** (`.github/workflows/deploy.yml`):
   - Deploys infrastructure (on-demand)
   - Updates container apps with new images
   - Runs integration tests

#### Manual Deployment
```bash
# Build and push
az acr build \
  --registry yourregistry \
  --image projectpilot-api:$(Build.BuildId) \
  --file src/ProjectPilot.Web/Dockerfile .

# Deploy
az containerapp update \
  --name projectpilot-api \
  --resource-group $AZURE_RESOURCE_GROUP \
  --image yourregistry.azurecr.io/projectpilot-api:$(Build.BuildId)
```

## Monitoring and Maintenance

### Health Checks
- **API Health**: `https://your-app.azurecontainerapps.io/health`
- **Container Health**: Check Azure portal or CLI

### Logging
```bash
# View application logs
az containerapp logs show \
  --name projectpilot-api \
  --resource-group $AZURE_RESOURCE_GROUP \
  --follow
```

### Scaling
```bash
# Manual scaling
az containerapp update \
  --name projectpilot-api \
  --resource-group $AZURE_RESOURCE_GROUP \
  --min-replicas 1 \
  --max-replicas 5
```

### Backup and Recovery
- **Cosmos DB**: Automatic backups enabled
- **Redis**: Configure persistence if needed
- **Container Registry**: Geo-redundant replication

## Troubleshooting

### Common Issues

#### Container App Not Starting
```bash
# Check logs
az containerapp logs show --name projectpilot-api --resource-group $AZURE_RESOURCE_GROUP

# Check environment variables
az containerapp show --name projectpilot-api --resource-group $AZURE_RESOURCE_GROUP --query properties.configuration.secrets
```

#### API Returning Errors
- Check LLM provider configuration
- Verify network connectivity to Azure services
- Review application logs for detailed error messages

#### High Resource Usage
- Monitor with Azure Monitor
- Adjust scaling rules
- Optimize container resource limits

### Performance Optimization

#### Container Resources
```bash
az containerapp update \
  --name projectpilot-api \
  --resource-group $AZURE_RESOURCE_GROUP \
  --cpu 0.5 \
  --memory 1.0Gi
```

#### Caching Strategy
- Redis for session data
- Cosmos DB for persistent memory
- CDN for static assets (if applicable)

## Security Considerations

### Network Security
- Use Azure Front Door or Application Gateway for additional security
- Configure network restrictions
- Enable Azure Defender

### Secret Management
- Store secrets in Azure Key Vault
- Use managed identities for service authentication
- Rotate keys regularly

### Compliance
- Enable Azure Policy for governance
- Configure audit logging
- Regular security assessments

## Cost Optimization

### Scale to Zero
- Configure minimum replicas to 0
- Use consumption-based pricing
- Monitor usage patterns

### Resource Rightsizing
- Start with minimal resources
- Monitor and adjust based on usage
- Use Azure Advisor recommendations

### Cleanup
```bash
# Remove resource group (for testing)
az group delete --name $AZURE_RESOURCE_GROUP --yes
```

## Support and Resources

- **Azure Documentation**: https://docs.microsoft.com/azure
- **Container Apps**: https://docs.microsoft.com/azure/container-apps
- **Project Issues**: Create GitHub issues for deployment problems

## Next Steps

- Configure monitoring dashboards
- Set up alerting rules
- Implement blue-green deployments
- Add automated testing in CI/CD pipeline