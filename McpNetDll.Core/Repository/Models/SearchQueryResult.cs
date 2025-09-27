namespace McpNetDll.Repository;

public class SearchQueryResult
{
    public List<SearchResult> Results { get; init; } = new();
    public PaginationInfo Pagination { get; init; } = new();
    public string? Error { get; init; }
}