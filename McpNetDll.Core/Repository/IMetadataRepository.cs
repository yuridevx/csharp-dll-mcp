namespace McpNetDll.Repository;

public interface IMetadataRepository
{
    NamespaceQueryResult QueryNamespaces(string[]? namespaces = null, int limit = 50, int offset = 0);
    TypeDetailsQueryResult QueryTypeDetails(string[] typeNames);
    SearchQueryResult SearchElements(string pattern, string searchScope = "all", int limit = 100, int offset = 0);
}