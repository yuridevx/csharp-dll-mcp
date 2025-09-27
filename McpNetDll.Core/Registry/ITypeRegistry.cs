namespace McpNetDll.Registry;

public interface ITypeRegistry
{
    void LoadAssemblies(string[] paths);
    void LoadAssembly(string path);
    List<TypeMetadata> GetAllTypes();
    TypeMetadata? GetTypeByFullName(string fullName);
    List<TypeMetadata> GetTypesBySimpleName(string simpleName);
    List<TypeMetadata> GetTypesByNamespace(string namespaceName);
    List<string> GetAllNamespaces();
    bool TryGetType(string name, out TypeMetadata? type);
    List<string> GetLoadErrors();
}