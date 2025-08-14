using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpNetDll;

[McpServerToolType]
public static class DllMetadataTool
{
    private static string GetNamespaceInfo(Extractor extractor)
    {
        var namespaces = extractor.GetAvailableNamespaces();
        if (!namespaces.Any()) return "";
        
        var summary = namespaces.Count <= 8 
            ? string.Join(", ", namespaces)
            : $"{string.Join(", ", namespaces.Take(8))}... (+{namespaces.Count - 8} more)";
        
        return $" Currently loaded namespaces ({namespaces.Count}): {summary}";
    }

    [McpServerTool,
     Description("Lists all public namespaces and their types from loaded .NET assemblies")]
    public static string ListNamespaces(
        Extractor extractor,
        [Description("Optional: An array of specific namespace names to inspect. If omitted, all loaded namespaces will be listed.")]
        string[]? namespaces = null)
    {
        var result = extractor.ListNamespaces(namespaces);
        
        // Add dynamic context to help Claude understand what's available
        if (namespaces == null || namespaces.Length == 0)
        {
            var availableNamespaces = extractor.GetAvailableNamespaces();
            var contextualResult = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(result);
            
            if (contextualResult.TryGetProperty("Namespaces", out var namespacesElement))
            {
                var enhancedResult = new
                {
                    Summary = $"Found {availableNamespaces.Count} namespaces in loaded assemblies",
                    LoadedAssemblyInfo = GetNamespaceInfo(extractor).Trim(),
                    Namespaces = namespacesElement
                };
                return System.Text.Json.JsonSerializer.Serialize(enhancedResult, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            }
        }
        
        return result;
    }

    [McpServerTool,
     Description("Gets detailed public API information for specific .NET types from loaded assemblies")]
    public static string GetTypeDetails(
        Extractor extractor,
        [Description("An array of type names to analyze. Use full names (e.g., 'MyNamespace.MyClass') or simple names if unambiguous. Use ListNamespaces to discover available types.")]
        string[] typeNames)
    {
        var result = extractor.GetTypeDetails(typeNames);
        
        // Add contextual information about what assemblies are loaded
        var contextualResult = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(result);
        if (contextualResult.TryGetProperty("Types", out var typesElement))
        {
            var enhancedResult = new
            {
                Summary = $"Type details for {typeNames.Length} requested type(s)",
                LoadedAssemblyInfo = GetNamespaceInfo(extractor).Trim(),
                Types = typesElement
            };
            return System.Text.Json.JsonSerializer.Serialize(enhancedResult, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
        
        return result;
    }
}