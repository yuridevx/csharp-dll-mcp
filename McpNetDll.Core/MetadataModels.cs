namespace McpNetDll;

public record AssemblyMetadata
{
    public required string Name { get; init; }
    public required List<TypeMetadata> Types { get; init; }
}

public record NamespaceMetadata
{
    public required string Name { get; init; }
    public required int TypeCount { get; init; }
    public required List<TypeMetadata> Types { get; init; }
}

public record TypeMetadata
{
    public required string Name { get; init; }
    public required string Namespace { get; init; }
    public required string TypeKind { get; init; }
    public string? Documentation { get; init; }
    public int? MethodCount { get; init; }
    public int? PropertyCount { get; init; }
    public int? FieldCount { get; init; }
    public List<MethodMetadata>? Methods { get; init; }
    public List<PropertyMetadata>? Properties { get; init; }
    public List<EnumValueMetadata>? EnumValues { get; init; }
    public StructLayoutMetadata? StructLayout { get; init; }
    public List<FieldMetadata>? Fields { get; init; }
}

public record MethodMetadata
{
    public required string Name { get; init; }
    public required string ReturnType { get; init; }
    public string? Documentation { get; init; }
    public bool IsStatic { get; init; }
    public required List<ParameterMetadata> Parameters { get; init; }
}

public record PropertyMetadata
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public string? Documentation { get; init; }
    public bool IsStatic { get; init; }
}

public record ParameterMetadata
{
    public required string Name { get; init; }
    public required string Type { get; init; }
}

public record EnumValueMetadata
{
    public required string Name { get; init; }
    public string? Value { get; init; }
}

public record FieldMetadata
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public int? Offset { get; init; }
    public string? Documentation { get; init; }
    public bool IsStatic { get; init; }
}

public record StructLayoutMetadata
{
    public required string Kind { get; init; }
    public int? Pack { get; init; }
    public int? Size { get; init; }
}