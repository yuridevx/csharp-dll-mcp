using Xunit;
using McpNetDll;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Reflection;

namespace McpNetDll.Tests;

public class ExtractorTests
{
    private readonly string _testDllPath;
    private readonly string _testXmlPath;
    private readonly Extractor _extractor;

    public ExtractorTests()
    {
        // Adjust the path to be relative to the test project's output directory
        // Assuming MyTestLibrary.dll is in the same output directory as the test assembly
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        _testDllPath = Path.Combine(assemblyLocation, "MyTestLibrary.dll");
        _testXmlPath = Path.ChangeExtension(_testDllPath, ".xml"); // Assuming XML documentation is also there

        _extractor = new Extractor();
    }

    [Fact]
    public void Extractor_ShouldReturnNamespaceInfo_WhenNoFiltersAreApplied()
    {
        // Arrange & Act
        var resultJson = _extractor.Extract(_testDllPath);
        var result = JsonSerializer.Deserialize<JsonElement>(resultJson);

        // Assert
        Assert.True(result.TryGetProperty("Namespaces", out var namespaces));
        Assert.Single(namespaces.EnumerateArray());

        var myTestLibraryNamespace = namespaces.EnumerateArray().First(ns => ns.GetProperty("Name").GetString() == "MyTestLibrary");
        Assert.NotNull(myTestLibraryNamespace);
        Assert.Equal(1, myTestLibraryNamespace.GetProperty("ClassCount").GetInt32());
        Assert.Contains("MyPublicClass", myTestLibraryNamespace.GetProperty("ClassNames").EnumerateArray().Select(c => c.GetString()));
    }

    [Fact]
    public void Extractor_ShouldReturnClassInfo_WhenFilteredByNamespace()
    {
        // Arrange
        string[] namespaces = { "MyTestLibrary" };

        // Act
        var resultJson = _extractor.ExtractWithFilters(_testDllPath, namespaces, null);
        var result = JsonSerializer.Deserialize<JsonElement>(resultJson);

        // Assert
        Assert.True(result.TryGetProperty("Types", out var types));
        Assert.Single(types.EnumerateArray());

        var myPublicClass = types.EnumerateArray().First(t => t.GetProperty("Name").GetString() == "MyPublicClass");
        Assert.NotNull(myPublicClass);
        Assert.Equal("MyTestLibrary", myPublicClass.GetProperty("Namespace").GetString());
        Assert.Equal("MyTestLibrary.MyPublicClass", myPublicClass.GetProperty("FullName").GetString());
        Assert.True(myPublicClass.GetProperty("MethodCount").GetInt32() > 0); // Check if methods are counted
        Assert.True(myPublicClass.GetProperty("PropertyCount").GetInt32() > 0); // Check if properties are counted
        Assert.False(myPublicClass.TryGetProperty("Methods", out _)); // Should not contain detailed methods
        Assert.False(myPublicClass.TryGetProperty("Properties", out _)); // Should not contain detailed properties
    }

    [Fact]
    public void Extractor_ShouldReturnMemberInfo_WhenFilteredByClassName()
    {
        // Arrange
        string[] classNames = { "MyTestLibrary.MyPublicClass" };

        // Act
        var resultJson = _extractor.ExtractWithFilters(_testDllPath, null, classNames);
        var result = JsonSerializer.Deserialize<JsonElement>(resultJson);

        // Assert
        Assert.True(result.TryGetProperty("Types", out var types));
        Assert.Single(types.EnumerateArray());

        var myPublicClass = types.EnumerateArray().First(t => t.GetProperty("Name").GetString() == "MyPublicClass");
        Assert.NotNull(myPublicClass);
        Assert.True(myPublicClass.TryGetProperty("Methods", out var methods)); // Should contain detailed methods
        Assert.True(myPublicClass.TryGetProperty("Properties", out var properties)); // Should contain detailed properties
        Assert.True(methods.EnumerateArray().Any(m => m.GetProperty("Name").GetString() == "GetInstanceMessage"));
        Assert.True(properties.EnumerateArray().Any(p => p.GetProperty("Name").GetString() == "InstanceProperty"));
    }

    [Fact]
    public void Extractor_ShouldReturnError_WhenAssemblyNotFound()
    {
        // Arrange
        var nonExistentPath = "C:\\NonExistent\\Assembly.dll";

        // Act
        var resultJson = _extractor.Extract(nonExistentPath);
        var result = JsonSerializer.Deserialize<JsonElement>(resultJson);

        // Assert
        Assert.True(result.TryGetProperty("error", out var error));
        Assert.Equal("Assembly file not found.", error.GetString());
    }

    [Fact]
    public void Extractor_ShouldReturnError_WhenBothFiltersAreProvided()
    {
        // Arrange
        string[] namespaces = { "Namespace1" };
        string[] classNames = { "Namespace1.Class1" };

        // Act
        var resultJson = _extractor.ExtractWithFilters(_testDllPath, namespaces, classNames);
        var result = JsonSerializer.Deserialize<JsonElement>(resultJson);

        // Assert
        Assert.True(result.TryGetProperty("error", out var error));
        Assert.Equal("Cannot specify both namespaces and classNames parameters. Use either namespaces for class-level info or classNames for member-level info.", error.GetString());
    }

    [Fact]
    public void Extractor_ShouldReturnError_WhenEmptyNamespacesArray()
    {
        // Arrange
        string[] namespaces = { };

        // Act
        var resultJson = _extractor.ExtractWithFilters(_testDllPath, namespaces, null);
        var result = JsonSerializer.Deserialize<JsonElement>(resultJson);

        // Assert
        Assert.True(result.TryGetProperty("error", out var error));
        Assert.Equal("Namespaces array cannot be empty.", error.GetString());
    }

    [Fact]
    public void Extractor_ShouldReturnError_WhenEmptyClassNamesArray()
    {
        // Arrange
        string[] classNames = { };

        // Act
        var resultJson = _extractor.ExtractWithFilters(_testDllPath, null, classNames);
        var result = JsonSerializer.Deserialize<JsonElement>(resultJson);

        // Assert
        Assert.True(result.TryGetProperty("error", out var error));
        Assert.Equal("ClassNames array cannot be empty.", error.GetString());
    }
    
    [Fact]
    public void Extractor_ShouldReturnError_WhenInvalidClassNameFormat()
    {
        // Arrange
        string[] classNames = { "InvalidClass" }; // Missing dot

        // Act
        var resultJson = _extractor.ExtractWithFilters(_testDllPath, null, classNames);
        var result = JsonSerializer.Deserialize<JsonElement>(resultJson);

        // Assert
        Assert.True(result.TryGetProperty("error", out var error));
        Assert.Equal("Invalid class name format: 'InvalidClass'. Expected format: 'Namespace.Class'.", error.GetString());
    }

    [Fact]
    public void Extractor_ShouldReturnError_WhenNamespaceNotFound()
    {
        // Arrange
        string[] namespaces = { "NonExistentNamespace" };

        // Act
        var resultJson = _extractor.ExtractWithFilters(_testDllPath, namespaces, null);
        var result = JsonSerializer.Deserialize<JsonElement>(resultJson);

        // Assert
        Assert.True(result.TryGetProperty("error", out var error));
        Assert.Contains("Namespace(s) not found: NonExistentNamespace", error.GetString());
    }

    [Fact]
    public void Extractor_ShouldReturnError_WhenClassNotFound()
    {
        // Arrange
        string[] classNames = { "MyTestLibrary.NonExistentClass" };

        // Act
        var resultJson = _extractor.ExtractWithFilters(_testDllPath, null, classNames);
        var result = JsonSerializer.Deserialize<JsonElement>(resultJson);

        // Assert
        Assert.True(result.TryGetProperty("error", out var error));
        Assert.Contains("Class(es) not found: MyTestLibrary.NonExistentClass", error.GetString());
    }
}