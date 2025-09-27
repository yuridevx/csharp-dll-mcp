using McpNetDll.Repository;

namespace McpNetDll;

public class NamespaceQueryResult
{
    public List<NamespaceMetadata> Namespaces { get; init; } = new();
    public PaginationInfo Pagination { get; init; } = new();
    public string? Error { get; init; }
}