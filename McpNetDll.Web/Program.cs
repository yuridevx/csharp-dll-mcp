using McpNetDll.Registry;
using McpNetDll.Repository;
using McpNetDll.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddSingleton<ITypeRegistry>(sp =>
{
    var reg = new TypeRegistry();
    var dlls = builder.Configuration.GetSection("DllPaths").Get<string[]>() ?? Array.Empty<string>();
    reg.LoadAssemblies(dlls);
    return reg;
});
builder.Services.AddSingleton<IMetadataRepository, MetadataRepository>();
builder.Services.AddSingleton<IMcpResponseFormatter, McpResponseFormatter>();

var app = builder.Build();

// Static UI
app.UseDefaultFiles();
app.UseStaticFiles();

// Operational APIs
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

app.MapPost("/api/load", (ITypeRegistry registry, string path) =>
{
    if (string.IsNullOrWhiteSpace(path))
        return Results.BadRequest(new { error = "Path is required" });

    if (registry is TypeRegistry concrete)
    {
        concrete.LoadAssembly(PathHelper.ConvertWslPath(path));
        return Results.Json(new
        {
            message = "Loaded",
            path,
            namespaces = concrete.GetAllNamespaces().Count,
            types = concrete.GetAllTypes().Count,
            errors = concrete.GetLoadErrors()
        });
    }
    return Results.Problem("Registry is not mutable in this configuration");
});

// Lightweight namespaces list for tree building
app.MapGet("/api/namespaces/list", (ITypeRegistry registry)
    => Results.Json(registry.GetAllNamespaces()));

app.MapGet("/api/namespaces", (IMetadataRepository repo, IMcpResponseFormatter formatter, ITypeRegistry registry, string[]? namespaces, int? limit, int? offset)
    => Results.Text(formatter.FormatNamespaceResponse(repo.QueryNamespaces(namespaces, limit ?? 50, offset ?? 0), registry), "application/json"));

app.MapGet("/api/types", (IMetadataRepository repo, IMcpResponseFormatter formatter, ITypeRegistry registry, string[] typeNames)
    => Results.Text(formatter.FormatTypeDetailsResponse(repo.QueryTypeDetails(typeNames), registry), "application/json"));

app.MapGet("/api/search", (IMetadataRepository repo, IMcpResponseFormatter formatter, ITypeRegistry registry, string pattern, string? scope, int? limit, int? offset)
    => Results.Text(formatter.FormatSearchResponse(repo.SearchElements(pattern, scope ?? "all", limit ?? 100, offset ?? 0), registry), "application/json"));

// All known type full names (for linkability decisions in UI)
app.MapGet("/api/types/list", (ITypeRegistry registry)
    => Results.Json(registry.GetAllTypes().Select(t => $"{t.Namespace}.{t.Name}")));

app.Run();
