using System.Collections.Generic;
using System.Linq;
using McpNetDll;
using McpNetDll.Registry;
using McpNetDll.Repository;
using Moq;
using Xunit;

namespace McpNetDll.Tests.Repository;

public class MetadataRepositoryTests
{
    private Mock<ITypeRegistry> CreateMockRegistry()
    {
        return new Mock<ITypeRegistry>();
    }

    private TypeMetadata CreateTestTypeMetadata(string name = "TestClass", string ns = "TestNamespace")
    {
        return new TypeMetadata
        {
            Name = name,
            Namespace = ns,
            TypeKind = "class",
            MethodCount = 2,
            PropertyCount = 1,
            Methods = new List<MethodMetadata>
            {
                new() { Name = "Method1", ReturnType = "System.Void", Parameters = new List<ParameterMetadata>() },
                new() { Name = "Method2", ReturnType = "System.String", Parameters = new List<ParameterMetadata>() }
            },
            Properties = new List<PropertyMetadata>
            {
                new() { Name = "Property1", Type = "System.Int32" }
            }
        };
    }

    [Fact]
    public void QueryNamespaces_WithNoTypes_ReturnsEmptyResult()
    {
        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllTypes()).Returns(new List<TypeMetadata>());
        mockRegistry.Setup(r => r.GetLoadErrors()).Returns(new List<string>());

        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.QueryNamespaces();

        Assert.NotNull(result);
        Assert.Empty(result.Namespaces);
        Assert.Null(result.Error);
        Assert.Equal(0, result.Pagination.Total);
    }

    [Fact]
    public void QueryNamespaces_WithLoadErrors_ReturnsError()
    {
        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllTypes()).Returns(new List<TypeMetadata>());
        mockRegistry.Setup(r => r.GetLoadErrors()).Returns(new List<string> { "Error1", "Error2" });

        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.QueryNamespaces();

        Assert.NotNull(result);
        Assert.NotNull(result.Error);
        Assert.Contains("Failed to load all assemblies", result.Error);
    }

    [Fact]
    public void QueryNamespaces_WithTypes_GroupsByNamespace()
    {
        var types = new List<TypeMetadata>
        {
            CreateTestTypeMetadata("Class1", "Namespace1"),
            CreateTestTypeMetadata("Class2", "Namespace1"),
            CreateTestTypeMetadata("Class3", "Namespace2")
        };

        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllTypes()).Returns(types);
        mockRegistry.Setup(r => r.GetLoadErrors()).Returns(new List<string>());

        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.QueryNamespaces();

        Assert.NotNull(result);
        Assert.Equal(2, result.Namespaces.Count);
        Assert.Equal("Namespace1", result.Namespaces[0].Name);
        Assert.Equal(2, result.Namespaces[0].TypeCount);
        Assert.Equal("Namespace2", result.Namespaces[1].Name);
        Assert.Equal(1, result.Namespaces[1].TypeCount);
    }

    [Fact]
    public void QueryNamespaces_WithPagination_ReturnsCorrectPage()
    {
        var types = Enumerable.Range(1, 10)
            .Select(i => CreateTestTypeMetadata($"Class{i}", $"Namespace{i}"))
            .ToList();

        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllTypes()).Returns(types);
        mockRegistry.Setup(r => r.GetLoadErrors()).Returns(new List<string>());

        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.QueryNamespaces(null, limit: 3, offset: 2);

        Assert.NotNull(result);
        Assert.Equal(3, result.Namespaces.Count);
        Assert.Equal(10, result.Pagination.Total);
        Assert.Equal(3, result.Pagination.Limit);
        Assert.Equal(2, result.Pagination.Offset);
        Assert.True(result.Pagination.HasMore);
    }

    [Fact]
    public void QueryNamespaces_WithSpecificNamespaces_FiltersResults()
    {
        var types = new List<TypeMetadata>
        {
            CreateTestTypeMetadata("Class1", "Namespace1"),
            CreateTestTypeMetadata("Class2", "Namespace2"),
            CreateTestTypeMetadata("Class3", "Namespace3")
        };

        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllTypes()).Returns(types);
        mockRegistry.Setup(r => r.GetAllNamespaces()).Returns(new List<string> { "Namespace1", "Namespace2", "Namespace3" });
        mockRegistry.Setup(r => r.GetTypesByNamespace("Namespace1")).Returns(new List<TypeMetadata> { types[0] });
        mockRegistry.Setup(r => r.GetTypesByNamespace("Namespace3")).Returns(new List<TypeMetadata> { types[2] });
        mockRegistry.Setup(r => r.GetLoadErrors()).Returns(new List<string>());

        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.QueryNamespaces(new[] { "Namespace1", "Namespace3" });

        Assert.NotNull(result);
        Assert.Equal(2, result.Namespaces.Count);
        Assert.Contains(result.Namespaces, ns => ns.Name == "Namespace1");
        Assert.Contains(result.Namespaces, ns => ns.Name == "Namespace3");
        Assert.DoesNotContain(result.Namespaces, ns => ns.Name == "Namespace2");
    }

    [Fact]
    public void QueryNamespaces_WithNonExistentNamespace_ReturnsError()
    {
        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllTypes()).Returns(new List<TypeMetadata>());
        mockRegistry.Setup(r => r.GetAllNamespaces()).Returns(new List<string> { "ExistingNamespace" });
        mockRegistry.Setup(r => r.GetLoadErrors()).Returns(new List<string>());

        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.QueryNamespaces(new[] { "NonExistentNamespace" });

        Assert.NotNull(result);
        Assert.NotNull(result.Error);
        Assert.Contains("Namespace(s) not found", result.Error);
    }

    [Fact]
    public void QueryTypeDetails_WithEmptyArray_ReturnsError()
    {
        var mockRegistry = CreateMockRegistry();
        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.QueryTypeDetails(new string[0]);

        Assert.NotNull(result);
        Assert.NotNull(result.Error);
        Assert.Contains("TypeNames array cannot be empty", result.Error);
    }

    [Fact]
    public void QueryTypeDetails_WithNullArray_ReturnsError()
    {
        var mockRegistry = CreateMockRegistry();
        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.QueryTypeDetails(null!);

        Assert.NotNull(result);
        Assert.NotNull(result.Error);
        Assert.Contains("TypeNames array cannot be empty", result.Error);
    }

    [Fact]
    public void QueryTypeDetails_WithExistingTypes_ReturnsTypes()
    {
        var type1 = CreateTestTypeMetadata("Class1", "Namespace1");
        var type2 = CreateTestTypeMetadata("Class2", "Namespace2");

        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllTypes()).Returns(new List<TypeMetadata> { type1, type2 });
        mockRegistry.Setup(r => r.GetLoadErrors()).Returns(new List<string>());
        mockRegistry.Setup(r => r.TryGetType("Namespace1.Class1", out type1)).Returns(true);
        mockRegistry.Setup(r => r.TryGetType("Namespace2.Class2", out type2)).Returns(true);

        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.QueryTypeDetails(new[] { "Namespace1.Class1", "Namespace2.Class2" });

        Assert.NotNull(result);
        Assert.Null(result.Error);
        Assert.Equal(2, result.Types.Count);
        Assert.Contains(result.Types, t => t.Name == "Class1");
        Assert.Contains(result.Types, t => t.Name == "Class2");
    }

    [Fact]
    public void QueryTypeDetails_WithNonExistentType_ReturnsError()
    {
        TypeMetadata? nullType = null;

        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllTypes()).Returns(new List<TypeMetadata>());
        mockRegistry.Setup(r => r.GetLoadErrors()).Returns(new List<string>());
        mockRegistry.Setup(r => r.TryGetType("NonExistent.Type", out nullType)).Returns(false);

        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.QueryTypeDetails(new[] { "NonExistent.Type" });

        Assert.NotNull(result);
        Assert.NotNull(result.Error);
        Assert.Contains("Type(s) not found or ambiguous", result.Error);
        Assert.NotNull(result.AvailableTypes);
    }

    [Fact]
    public void SearchElements_WithInvalidRegex_ReturnsError()
    {
        var mockRegistry = CreateMockRegistry();
        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.SearchElements("[invalid(regex");

        Assert.NotNull(result);
        Assert.NotNull(result.Error);
        Assert.Contains("Invalid regex pattern", result.Error);
    }

    [Fact]
    public void SearchElements_SearchesInTypes()
    {
        var types = new List<TypeMetadata>
        {
            CreateTestTypeMetadata("ServiceClass", "TestNamespace"),
            CreateTestTypeMetadata("RepositoryClass", "TestNamespace")
        };

        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllTypes()).Returns(types);

        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.SearchElements("Service", "types");

        Assert.NotNull(result);
        Assert.Null(result.Error);
        Assert.Single(result.Results);
        Assert.Equal("ServiceClass", result.Results[0].Name);
        Assert.Equal("Type", result.Results[0].ElementType);
    }

    [Fact]
    public void SearchElements_SearchesInMethods()
    {
        var type = CreateTestTypeMetadata();
        type = new TypeMetadata
        {
            Name = type.Name,
            Namespace = type.Namespace,
            TypeKind = type.TypeKind,
            MethodCount = 2,
            PropertyCount = type.PropertyCount,
            Methods = new List<MethodMetadata>
            {
                new() { Name = "GetData", ReturnType = "System.String", Parameters = new List<ParameterMetadata>() },
                new() { Name = "SetData", ReturnType = "System.Void", Parameters = new List<ParameterMetadata>() }
            },
            Properties = type.Properties
        };

        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllTypes()).Returns(new List<TypeMetadata> { type });

        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.SearchElements("Get", "methods");

        Assert.NotNull(result);
        Assert.Single(result.Results);
        Assert.Equal("GetData", result.Results[0].Name);
        Assert.Equal("Method", result.Results[0].ElementType);
    }

    [Fact]
    public void SearchElements_SearchesInProperties()
    {
        var type = CreateTestTypeMetadata();
        type = new TypeMetadata
        {
            Name = type.Name,
            Namespace = type.Namespace,
            TypeKind = type.TypeKind,
            MethodCount = type.MethodCount,
            PropertyCount = 2,
            Methods = type.Methods,
            Properties = new List<PropertyMetadata>
            {
                new() { Name = "Count", Type = "System.Int32" },
                new() { Name = "Name", Type = "System.String" }
            }
        };

        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllTypes()).Returns(new List<TypeMetadata> { type });

        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.SearchElements("Count", "properties");

        Assert.NotNull(result);
        Assert.Single(result.Results);
        Assert.Equal("Count", result.Results[0].Name);
        Assert.Equal("Property", result.Results[0].ElementType);
    }

    [Fact]
    public void SearchElements_SearchesInFields()
    {
        var type = CreateTestTypeMetadata();
        type = new TypeMetadata
        {
            Name = type.Name,
            Namespace = type.Namespace,
            TypeKind = type.TypeKind,
            MethodCount = type.MethodCount,
            PropertyCount = type.PropertyCount,
            FieldCount = 2,
            Methods = type.Methods,
            Properties = type.Properties,
            Fields = new List<FieldMetadata>
            {
                new() { Name = "_data", Type = "System.String" },
                new() { Name = "_count", Type = "System.Int32" }
            }
        };

        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllTypes()).Returns(new List<TypeMetadata> { type });

        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.SearchElements("_data", "fields");

        Assert.NotNull(result);
        Assert.Single(result.Results);
        Assert.Equal("_data", result.Results[0].Name);
        Assert.Equal("Field", result.Results[0].ElementType);
    }

    [Fact]
    public void SearchElements_SearchesInEnumValues()
    {
        var type = CreateTestTypeMetadata();
        type = new TypeMetadata
        {
            Name = type.Name,
            Namespace = type.Namespace,
            TypeKind = "enum",
            MethodCount = type.MethodCount,
            PropertyCount = type.PropertyCount,
            Methods = type.Methods,
            Properties = type.Properties,
            EnumValues = new List<EnumValueMetadata>
            {
                new() { Name = "None", Value = "0" },
                new() { Name = "Active", Value = "1" },
                new() { Name = "Inactive", Value = "2" }
            }
        };

        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllTypes()).Returns(new List<TypeMetadata> { type });

        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.SearchElements("^Active$", "enums");

        Assert.NotNull(result);
        Assert.Single(result.Results);
        Assert.Equal("Active", result.Results[0].Name);
        Assert.Equal("EnumValue", result.Results[0].ElementType);
        Assert.Equal("1", result.Results[0].Value);
    }

    [Fact]
    public void SearchElements_WithPagination_ReturnsCorrectPage()
    {
        var types = Enumerable.Range(1, 20)
            .Select(i => CreateTestTypeMetadata($"Class{i}", "TestNamespace"))
            .ToList();

        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllTypes()).Returns(types);

        var repository = new MetadataRepository(mockRegistry.Object);

        var result = repository.SearchElements("Class", "types", limit: 5, offset: 10);

        Assert.NotNull(result);
        Assert.Equal(5, result.Results.Count);
        Assert.Equal(20, result.Pagination.Total);
        Assert.Equal(5, result.Pagination.Limit);
        Assert.Equal(10, result.Pagination.Offset);
        Assert.True(result.Pagination.HasMore);
    }
}