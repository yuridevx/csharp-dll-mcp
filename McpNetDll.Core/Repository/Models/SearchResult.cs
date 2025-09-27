namespace McpNetDll.Repository;

public class SearchResult
{
    public required string ElementType { get; init; }
    public required string Name { get; init; }
    public string? Namespace { get; init; }
    public string? ParentType { get; init; }
    public string? TypeKind { get; init; }
    public string? FullName { get; init; }
    public string? ReturnType { get; init; }
    public string? PropertyType { get; init; }
    public string? FieldType { get; init; }
    public string? Value { get; init; }
    public IEnumerable<string>? Parameters { get; init; }
}