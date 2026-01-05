using System.ComponentModel.DataAnnotations;

namespace ProjectPilot.LLM;

public class LLMOptions
{
    public const string SectionName = "LLM";

    [Required]
    public string DefaultProvider { get; set; } = "AzureOpenAI";

    public AzureOpenAIOptions AzureOpenAI { get; set; } = new();
    public GeminiOptions Gemini { get; set; } = new();
    public MemoryOptions Memory { get; set; } = new();
}

public class AzureOpenAIOptions
{
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    public string DeploymentName { get; set; } = "gpt-4";

    public string ApiVersion { get; set; } = "2024-02-15-preview";
}

public class GeminiOptions
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    public string ProjectId { get; set; } = string.Empty;

    public string Location { get; set; } = "us-central1";

    public string Model { get; set; } = "gemini-1.5-pro";
}

public class MemoryOptions
{
    public string DefaultStore { get; set; } = "Redis";
    public RedisMemoryOptions Redis { get; set; } = new();
    public CosmosMemoryOptions Cosmos { get; set; } = new();
}