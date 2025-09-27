using McpNetDll.Core;
using McpNetDll.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);
var dlls = builder.Configuration.GetSection("DllPaths").Get<string[]>() ?? Array.Empty<string>();

// Services
builder.Services.AddCoreServices(dlls);

var app = builder.Build();

// Static UI
app.UseDefaultFiles();
app.UseStaticFiles();

// Operational APIs
app.MapInfoEndpoints();
app.MapLoadEndpoints();
app.MapNamespaceEndpoints();
app.MapTypeEndpoints();
app.MapSearchEndpoints();

app.Run();