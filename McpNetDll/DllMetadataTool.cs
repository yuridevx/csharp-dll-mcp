using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpNetDll;

[McpServerToolType]
public static class DllMetadataTool
{
    [McpServerTool,
     Description("Lists all public namespaces in the assembly. This is the recommended starting point for exploring the library's structure.")]
    public static string ListNamespaces(
        Extractor extractor,
        [Description("The absolute path to the DLL file.")]
        string dllPath)
    {
        return extractor.ListNamespaces(dllPath);
    }

    [McpServerTool,
     Description("Lists all public .NET types (including classes, structs, enums, and interfaces) within the specified namespaces.")]
    public static string ListTypesInNamespaces(
        Extractor extractor,
        [Description("The absolute path to the DLL file.")]
        string dllPath,
        [Description("An array of namespace names to inspect. Use `ListNamespaces` to discover available namespaces.")]
        string[] namespaces)
    {
        return extractor.ListTypesInNamespaces(dllPath, namespaces);
    }

    [McpServerTool,
     Description("Gets detailed public API information for one or more .NET types (including classes, structs, enums, and interfaces), including methods, properties, and other members. You can use full or simple type names.")]
    public static string GetTypeDetails(
        Extractor extractor,
        [Description("The absolute path to the DLL file.")]
        string dllPath,
        [Description("An array of type names (e.g., 'MyClass', 'MyNamespace.MyClass') to get details for. Use `ListTypesInNamespaces` to discover types.")]
        string[] typeNames)
    {
        return extractor.GetTypeDetails(dllPath, typeNames);
    }
}