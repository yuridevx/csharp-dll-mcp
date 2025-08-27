using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using McpNetDll;
using McpNetDll.Registry;
using Xunit;

namespace McpNetDll.Tests.Registry;

public class TypeRegistryTests
{
    private TypeRegistry CreateRegistry()
    {
        return new TypeRegistry();
    }

    [Fact]
    public void GetAllTypes_WhenEmpty_ReturnsEmptyList()
    {
        var registry = CreateRegistry();
        
        var result = registry.GetAllTypes();
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllNamespaces_WhenEmpty_ReturnsEmptyList()
    {
        var registry = CreateRegistry();
        
        var result = registry.GetAllNamespaces();
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetLoadErrors_WhenNoErrors_ReturnsEmptyList()
    {
        var registry = CreateRegistry();
        
        var result = registry.GetLoadErrors();
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void LoadAssembly_WithInvalidPath_AddsLoadError()
    {
        var registry = CreateRegistry();
        var invalidPath = "NonExistentFile.dll";
        
        registry.LoadAssembly(invalidPath);
        
        var errors = registry.GetLoadErrors();
        Assert.Single(errors);
        Assert.Contains(invalidPath, errors[0]);
    }

    [Fact]
    public void LoadAssemblies_WithMultipleInvalidPaths_AddsMultipleErrors()
    {
        var registry = CreateRegistry();
        var invalidPaths = new[] { "File1.dll", "File2.dll", "File3.dll" };
        
        registry.LoadAssemblies(invalidPaths);
        
        var errors = registry.GetLoadErrors();
        Assert.Equal(3, errors.Count);
    }

    [Fact]
    public void GetTypeByFullName_WhenTypeNotExists_ReturnsNull()
    {
        var registry = CreateRegistry();
        
        var result = registry.GetTypeByFullName("NonExistent.Type");
        
        Assert.Null(result);
    }

    [Fact]
    public void GetTypesBySimpleName_WhenTypeNotExists_ReturnsEmptyList()
    {
        var registry = CreateRegistry();
        
        var result = registry.GetTypesBySimpleName("NonExistentType");
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetTypesByNamespace_WhenNamespaceNotExists_ReturnsEmptyList()
    {
        var registry = CreateRegistry();
        
        var result = registry.GetTypesByNamespace("NonExistent.Namespace");
        
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void TryGetType_WithFullName_WhenNotExists_ReturnsFalse()
    {
        var registry = CreateRegistry();
        
        var result = registry.TryGetType("NonExistent.Type", out var type);
        
        Assert.False(result);
        Assert.Null(type);
    }

    [Fact]
    public void TryGetType_WithSimpleName_WhenNotExists_ReturnsFalse()
    {
        var registry = CreateRegistry();
        
        var result = registry.TryGetType("NonExistentType", out var type);
        
        Assert.False(result);
        Assert.Null(type);
    }

    [Fact]
    public void GetAllTypes_ReturnsNewListInstance()
    {
        var registry = CreateRegistry();
        
        var list1 = registry.GetAllTypes();
        var list2 = registry.GetAllTypes();
        
        Assert.NotSame(list1, list2);
    }

    [Fact]
    public void GetTypesBySimpleName_ReturnsNewListInstance()
    {
        var registry = CreateRegistry();
        
        var list1 = registry.GetTypesBySimpleName("Test");
        var list2 = registry.GetTypesBySimpleName("Test");
        
        Assert.NotSame(list1, list2);
    }

    [Fact]
    public void GetTypesByNamespace_ReturnsNewListInstance()
    {
        var registry = CreateRegistry();
        
        var list1 = registry.GetTypesByNamespace("Test");
        var list2 = registry.GetTypesByNamespace("Test");
        
        Assert.NotSame(list1, list2);
    }

    [Fact]
    public void GetLoadErrors_ReturnsNewListInstance()
    {
        var registry = CreateRegistry();
        
        var list1 = registry.GetLoadErrors();
        var list2 = registry.GetLoadErrors();
        
        Assert.NotSame(list1, list2);
    }
}