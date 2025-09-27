using McpNetDll.Helpers;
using McpNetDll.Registry;
using McpNetDll.Repository;

namespace McpNetDll.Web.Endpoints;

public static class TypeEndpoints
{
    public static void MapTypeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/types", (IMetadataRepository repo, IMcpResponseFormatter formatter, ITypeRegistry registry,
                string[] typeNames)
            => Results.Text(formatter.FormatTypeDetailsResponse(repo.QueryTypeDetails(typeNames), registry),
                "application/json"));

        // All known type full names (for linkability decisions in UI)
        app.MapGet("/api/types/list", (ITypeRegistry registry)
            => Results.Json(registry.GetAllTypes().Select(t => $"{t.Namespace}.{t.Name}")));
    }
}