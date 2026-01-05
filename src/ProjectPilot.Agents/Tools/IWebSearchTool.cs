using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectPilot.Agents.Tools;

public interface IWebSearchTool
{
    Task<List<SearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default);
}

public record SearchResult
{
    public required string Title { get; init; }
    public required string Url { get; init; }
    public required string Snippet { get; init; }
    public string? Content { get; init; }
}