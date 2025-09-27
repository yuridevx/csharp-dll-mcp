using McpNetDll.Helpers;
using McpNetDll.Registry;

namespace McpNetDll.Web.Endpoints;

public static class LoadEndpoints
{
    public static void MapLoadEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/load", (ITypeRegistry registry, string path) =>
        {
            if (string.IsNullOrWhiteSpace(path))
                return Results.BadRequest(new { error = "Path is required" });

            registry.LoadAssembly(PathHelper.ConvertWslPath(path));
            return Results.Json(new
            {
                message = "Loaded",
                path,
                namespaces = registry.GetAllNamespaces().Count,
                types = registry.GetAllTypes().Count,
                errors = registry.GetLoadErrors()
            });
        });
    }
}