# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/ProjectPilot.Web/ProjectPilot.Web.csproj", "src/ProjectPilot.Web/"]
COPY ["src/ProjectPilot.Agents/ProjectPilot.Agents.csproj", "src/ProjectPilot.Agents/"]
COPY ["src/ProjectPilot.LLM/ProjectPilot.LLM.csproj", "src/ProjectPilot.LLM/"]
RUN dotnet restore "src/ProjectPilot.Web/ProjectPilot.Web.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/ProjectPilot.Web"
RUN dotnet build "ProjectPilot.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ProjectPilot.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProjectPilot.Web.dll"]