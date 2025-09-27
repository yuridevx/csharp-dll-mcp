using McpNetDll.Helpers;
using McpNetDll.Registry;
using McpNetDll.Repository;

namespace McpNetDll.Web.Endpoints;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/search", (IMetadataRepository repo, IMcpResponseFormatter formatter, ITypeRegistry registry,
                string pattern, string? scope, int? limit, int? offset)
            => Results.Text(
                formatter.FormatSearchResponse(repo.SearchElements(pattern, scope ?? "all", limit ?? 100, offset ?? 0),
                    registry), "application/json"));
    }
}