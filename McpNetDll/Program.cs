using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace McpNetDll;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Redirect console logging to stderr
        builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

        builder.Services.AddSingleton<Extractor>();
        builder.Services
            .AddMcpServer(server => { server.ServerInfo = new() { Name = "McpNetDll", Version = "1.0.0" }; })
            .WithStdioServerTransport()
            .WithToolsFromAssembly(); // Scans the assembly for [McpServerToolType]

        var host = builder.Build();
        await host.RunAsync();
    }
}