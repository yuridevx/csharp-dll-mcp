using dnlib.DotNet;
using System.Text.Json;
using System.Xml.Linq;
using System;

namespace McpNetDll;

public class Extractor
{
    public string Extract(string assemblyPath)
    {
        return ExtractWithFilters(assemblyPath, null, null);
    }

    public string ExtractWithFilters(string assemblyPath, string[]? namespaces, string[]? classNames)
    {
        assemblyPath = ConvertWslPathToWindowsPath(assemblyPath);

        if (!File.Exists(assemblyPath))
        {
            return JsonSerializer.Serialize(new { error = "Assembly file not found." });
        }

        // Validate parameters
        if (namespaces != null && classNames != null)
        {
            return JsonSerializer.Serialize(new { error = "Cannot specify both namespaces and classNames parameters. Use either namespaces for class-level info or classNames for member-level info." });
        }

        if (namespaces != null && namespaces.Length == 0)
        {
            return JsonSerializer.Serialize(new { error = "Namespaces array cannot be empty." });
        }

        if (classNames != null && classNames.Length == 0)
        {
            return JsonSerializer.Serialize(new { error = "ClassNames array cannot be empty." });
        }

        // Validate class name format
        if (classNames != null)
        {
            foreach (var className in classNames)
            {
                if (string.IsNullOrWhiteSpace(className) || !className.Contains('.'))
                {
                    return JsonSerializer.Serialize(new { error = $"Invalid class name format: '{className}'. Expected format: 'Namespace.Class'." });
                }
            }
        }

        var xmlDocPath = Path.ChangeExtension(assemblyPath, ".xml");
        var comments = LoadComments(xmlDocPath);

        var modCtx = ModuleDef.CreateModuleContext();
        var module = ModuleDefMD.Load(assemblyPath, modCtx);
        var allTypes = module.Types
            .Where(t => t.IsPublic && (t.IsClass || t.IsInterface))
            .ToList();

        // Mode 1: No filters - return namespace information
        if (namespaces == null && classNames == null)
        {
            var namespaceInfo = allTypes
                .GroupBy(t => t.Namespace.String)
                .Select(g => new NamespaceMetadata
                {
                    Name = g.Key,
                    ClassCount = g.Count(),
                    ClassNames = g.Select(t => t.Name.String).ToList()
                })
                .OrderBy(ns => ns.Name)
                .ToList();

            return JsonSerializer.Serialize(new { Namespaces = namespaceInfo }, 
                new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
        }

        // Mode 2: Filter by namespaces - return class information
        if (namespaces != null && classNames == null)
        {
            var availableNamespaces = allTypes.Select(t => t.Namespace.String).Distinct().ToList();
            var notFoundNamespaces = namespaces.Where(ns => !availableNamespaces.Contains(ns)).ToList();
            
            if (notFoundNamespaces.Any())
            {
                return JsonSerializer.Serialize(new { 
                    error = $"Namespace(s) not found: {string.Join(", ", notFoundNamespaces)}",
                    availableNamespaces = availableNamespaces.OrderBy(ns => ns).ToList()
                });
            }

            var filteredTypes = allTypes
                .Where(t => namespaces.Contains(t.Namespace.String))
                .Select(type => new TypeMetadata
                {
                    Name = type.Name.String,
                    Namespace = type.Namespace.String,
                    FullName = type.FullName,
                    Documentation = comments.GetValueOrDefault(GetXmlDocKey(type)),
                    MethodCount = type.Methods.Count(m => m.IsPublic && !m.IsConstructor && !m.IsGetter && !m.IsSetter),
                    PropertyCount = type.Properties.Count(p => p.GetMethods.Any(gm => gm?.IsPublic ?? false)),
                    Methods = null,
                    Properties = null
                })
                .OrderBy(t => t.Namespace)
                .ThenBy(t => t.Name)
                .ToList();

            return JsonSerializer.Serialize(new { Types = filteredTypes }, 
                new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
        }

        // Mode 3: Filter by class names - return detailed member information
        if (classNames != null)
        {
            var availableClasses = allTypes.Select(t => t.FullName).ToList();
            var notFoundClasses = classNames.Where(cn => !availableClasses.Contains(cn)).ToList();
            
            if (notFoundClasses.Any())
            {
                return JsonSerializer.Serialize(new { 
                    error = $"Class(es) not found: {string.Join(", ", notFoundClasses)}",
                    availableClasses = availableClasses.OrderBy(cn => cn).ToList()
                });
            }

            var filteredTypes = allTypes
                .Where(t => classNames.Contains(t.FullName))
                .Select(type => new TypeMetadata
                {
                    Name = type.Name.String,
                    Namespace = type.Namespace.String,
                    FullName = type.FullName,
                    Documentation = comments.GetValueOrDefault(GetXmlDocKey(type)),
                    MethodCount = null,
                    PropertyCount = null,
                    Methods = type.Methods
                        .Where(m => m.IsPublic && !m.IsConstructor && !m.IsGetter && !m.IsSetter)
                        .Select(method => new MethodMetadata
                        {
                            Name = method.Name.String,
                            ReturnType = method.MethodSig.RetType.FullName,
                            Documentation = comments.GetValueOrDefault(GetXmlDocKey(method)),
                            Parameters = method.Parameters.Select(p => new ParameterMetadata
                            {
                                Name = p.Name,
                                Type = p.Type.FullName
                            }).ToList()
                        }).ToList(),
                    Properties = type.Properties
                        .Where(p => p.GetMethods.Any(gm => gm?.IsPublic ?? false))
                        .Select(prop => new PropertyMetadata
                        {
                            Name = prop.Name.String,
                            Type = prop.PropertySig.RetType.FullName,
                            Documentation = comments.GetValueOrDefault(GetXmlDocKey(prop))
                        }).ToList()
                }).ToList();

            return JsonSerializer.Serialize(new { Types = filteredTypes }, 
                new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
        }

        return JsonSerializer.Serialize(new { error = "Invalid filter parameters." });
    }

    private string ConvertWslPathToWindowsPath(string path)
    {
        if (OperatingSystem.IsWindows() && path.StartsWith("/mnt/") && path.Length > 6)
        {
            char driveLetter = path[5];
            string restOfPath = path.Substring(6);
            return $"{char.ToUpper(driveLetter)}:{restOfPath.Replace('/', '\\')}";
        }
        return path;
    }

    private Dictionary<string, string> LoadComments(string xmlPath)
    {
        if (!File.Exists(xmlPath))
        {
            return new Dictionary<string, string>();
        }

        try
        {
            var xdoc = XDocument.Load(xmlPath);
            return xdoc.Descendants("member")
                .ToDictionary(
                    member => member.Attribute("name")!.Value,
                    member => string.Concat(member.DescendantNodes().OfType<XText>()).Trim()
                );
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    private string GetXmlDocKey(TypeDef type) => $"T:{type.FullName}";

    private string GetXmlDocKey(MethodDef method)
    {
        var parameters = string.Join(",", method.Parameters.Select(p => p.Type.FullName));
        return $"M:{method.DeclaringType.FullName}.{method.Name.String}{(string.IsNullOrEmpty(parameters) ? "" : $"({parameters})")}";
    }

    private string GetXmlDocKey(PropertyDef prop) => $"P:{prop.DeclaringType.FullName}.{prop.Name.String}";
}

// Data model for serialization
public class AssemblyMetadata
{
    public required string Name { get; init; }
    public required List<TypeMetadata> Types { get; init; }
}

public class NamespaceMetadata
{
    public required string Name { get; init; }
    public required int ClassCount { get; init; }
    public required List<string> ClassNames { get; init; }
}

public class TypeMetadata
{
    public required string Name { get; init; }
    public required string Namespace { get; init; }
    public required string FullName { get; init; }
    public string? Documentation { get; init; }
    public int? MethodCount { get; init; }
    public int? PropertyCount { get; init; }
    public List<MethodMetadata>? Methods { get; init; }
    public List<PropertyMetadata>? Properties { get; init; }
}

public class MethodMetadata
{
    public required string Name { get; init; }
    public required string ReturnType { get; init; }
    public string? Documentation { get; init; }
    public required List<ParameterMetadata> Parameters { get; init; }
}

public class PropertyMetadata
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public string? Documentation { get; init; }
}

public class ParameterMetadata
{
    public required string Name { get; init; }
    public required string Type { get; init; }
} 