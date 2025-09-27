using McpNetDll.Registry;

namespace McpNetDll.Web.Endpoints;

public static class InfoEndpoints
{
    public static void MapInfoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/info", (ITypeRegistry registry) =>
        {
            var types = registry.GetAllTypes();
            var namespaces = registry.GetAllNamespaces();
            var errors = registry.GetLoadErrors();
            return Results.Json(new
            {
                Namespaces = namespaces.Count,
                Types = types.Count,
                LoadErrors = errors
            });
        });
    }
}