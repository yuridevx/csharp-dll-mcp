using System.Collections.Generic;
using System.Text.Json;
using McpNetDll;
using McpNetDll.Helpers;
using McpNetDll.Registry;
using McpNetDll.Repository;
using Moq;
using Xunit;

namespace McpNetDll.Tests.Helpers;

public class McpResponseFormatterTests
{
    private Mock<ITypeRegistry> CreateMockRegistry()
    {
        return new Mock<ITypeRegistry>();
    }

    private McpResponseFormatter CreateFormatter()
    {
        return new McpResponseFormatter();
    }

    [Fact]
    public void FormatNamespaceResponse_WithError_ReturnsErrorJson()
    {
        var formatter = CreateFormatter();
        var mockRegistry = CreateMockRegistry();
        var result = new NamespaceQueryResult { Error = "Test error message" };

        var json = formatter.FormatNamespaceResponse(result, mockRegistry.Object);

        Assert.NotNull(json);
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("error", out var errorProp));
        Assert.Equal("Test error message", errorProp.GetString());
    }

    [Fact]
    public void FormatNamespaceResponse_WithSuccess_IncludesSummaryAndNamespaces()
    {
        var formatter = CreateFormatter();
        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllNamespaces()).Returns(new List<string> { "Namespace1", "Namespace2" });

        var result = new NamespaceQueryResult
        {
            Namespaces = new List<NamespaceInfo>
            {
                new() { Name = "Namespace1", TypeCount = 2, Types = new List<TypeSummary>() }
            },
            Pagination = new PaginationInfo { Total = 1, Limit = 50, Offset = 0 }
        };

        var json = formatter.FormatNamespaceResponse(result, mockRegistry.Object);

        Assert.NotNull(json);
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("Summary", out var summaryProp));
        Assert.Contains("Found 1 namespaces", summaryProp.GetString());
        Assert.True(doc.RootElement.TryGetProperty("Namespaces", out _));
        Assert.True(doc.RootElement.TryGetProperty("Pagination", out _));
    }

    [Fact]
    public void FormatNamespaceResponse_WithManyNamespaces_ShowsFirstEight()
    {
        var formatter = CreateFormatter();
        var mockRegistry = CreateMockRegistry();
        var namespaces = new List<string>();
        for (int i = 1; i <= 15; i++)
        {
            namespaces.Add($"Namespace{i}");
        }
        mockRegistry.Setup(r => r.GetAllNamespaces()).Returns(namespaces);

        var result = new NamespaceQueryResult
        {
            Namespaces = new List<NamespaceInfo>(),
            Pagination = new PaginationInfo { Total = 15, Limit = 50, Offset = 0 }
        };

        var json = formatter.FormatNamespaceResponse(result, mockRegistry.Object);

        Assert.NotNull(json);
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("LoadedAssemblyInfo", out var infoProp));
        var info = infoProp.GetString();
        Assert.Contains("showing first 8", info);
        Assert.Contains("(+7 more)", info);
    }

    [Fact]
    public void FormatTypeDetailsResponse_WithError_ReturnsErrorJson()
    {
        var formatter = CreateFormatter();
        var mockRegistry = CreateMockRegistry();
        var result = new TypeDetailsQueryResult
        {
            Error = "Type not found",
            AvailableTypes = new List<string> { "Type1", "Type2" }
        };

        var json = formatter.FormatTypeDetailsResponse(result, mockRegistry.Object);

        Assert.NotNull(json);
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("error", out var errorProp));
        Assert.Equal("Type not found", errorProp.GetString());
        Assert.True(doc.RootElement.TryGetProperty("availableTypes", out _));
    }

    [Fact]
    public void FormatTypeDetailsResponse_WithSuccess_IncludesSummaryAndTypes()
    {
        var formatter = CreateFormatter();
        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllNamespaces()).Returns(new List<string> { "Namespace1" });

        var result = new TypeDetailsQueryResult
        {
            Types = new List<TypeMetadata>
            {
                new() { Name = "Class1", Namespace = "Namespace1", TypeKind = "class" },
                new() { Name = "Class2", Namespace = "Namespace1", TypeKind = "class" }
            }
        };

        var json = formatter.FormatTypeDetailsResponse(result, mockRegistry.Object);

        Assert.NotNull(json);
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("Summary", out var summaryProp));
        Assert.Contains("Type details for 2 requested type(s)", summaryProp.GetString());
        Assert.True(doc.RootElement.TryGetProperty("Types", out _));
        Assert.True(doc.RootElement.TryGetProperty("LoadedAssemblyInfo", out _));
    }

    [Fact]
    public void FormatSearchResponse_WithError_ReturnsErrorJson()
    {
        var formatter = CreateFormatter();
        var mockRegistry = CreateMockRegistry();
        var result = new SearchQueryResult { Error = "Invalid regex pattern" };

        var json = formatter.FormatSearchResponse(result, mockRegistry.Object);

        Assert.NotNull(json);
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("error", out var errorProp));
        Assert.Equal("Invalid regex pattern", errorProp.GetString());
    }

    [Fact]
    public void FormatSearchResponse_WithSuccess_IncludesSummaryAndResults()
    {
        var formatter = CreateFormatter();
        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllNamespaces()).Returns(new List<string> { "Namespace1" });

        var result = new SearchQueryResult
        {
            Results = new List<SearchResult>
            {
                new() { ElementType = "Type", Name = "TestClass" },
                new() { ElementType = "Method", Name = "TestMethod", ParentType = "TestClass" }
            },
            Pagination = new PaginationInfo { Total = 2, Limit = 100, Offset = 0 }
        };

        var json = formatter.FormatSearchResponse(result, mockRegistry.Object);

        Assert.NotNull(json);
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("Summary", out var summaryProp));
        Assert.Contains("Found 2 matching elements", summaryProp.GetString());
        Assert.True(doc.RootElement.TryGetProperty("Results", out _));
        Assert.True(doc.RootElement.TryGetProperty("Pagination", out _));
    }

    [Fact]
    public void FormatNamespaceResponse_WithNoNamespaces_ShowsEmptyInfo()
    {
        var formatter = CreateFormatter();
        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllNamespaces()).Returns(new List<string>());

        var result = new NamespaceQueryResult
        {
            Namespaces = new List<NamespaceInfo>(),
            Pagination = new PaginationInfo { Total = 0, Limit = 50, Offset = 0 }
        };

        var json = formatter.FormatNamespaceResponse(result, mockRegistry.Object);

        Assert.NotNull(json);
        var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.TryGetProperty("LoadedAssemblyInfo", out var infoProp));
        Assert.Equal("", infoProp.GetString());
    }

    [Fact]
    public void FormatResponse_ProducesValidIndentedJson()
    {
        var formatter = CreateFormatter();
        var mockRegistry = CreateMockRegistry();
        mockRegistry.Setup(r => r.GetAllNamespaces()).Returns(new List<string> { "Test.Namespace" });

        var result = new NamespaceQueryResult
        {
            Namespaces = new List<NamespaceInfo>
            {
                new() 
                { 
                    Name = "Test.Namespace", 
                    TypeCount = 1, 
                    Types = new List<TypeSummary>
                    {
                        new() { Name = "TestClass", Namespace = "Test.Namespace", TypeKind = "class" }
                    }
                }
            },
            Pagination = new PaginationInfo { Total = 1, Limit = 50, Offset = 0 }
        };

        var json = formatter.FormatNamespaceResponse(result, mockRegistry.Object);

        Assert.NotNull(json);
        Assert.Contains("\n", json); // Check for indentation (newlines)
        var doc = JsonDocument.Parse(json); // Will throw if invalid JSON
        Assert.NotNull(doc);
    }
}