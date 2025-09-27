namespace McpNetDll.Repository;

public class TypeDetailsQueryResult
{
    public List<TypeMetadata> Types { get; init; } = new();
    public string? Error { get; init; }
    public List<string>? AvailableTypes { get; init; }
}