using McpNetDll.Helpers;
using McpNetDll.Registry;
using McpNetDll.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace McpNetDll.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, string[] dllPaths, bool useAiFormatter = true)
    {
        services.AddSingleton<ITypeRegistry>(sp =>
        {
            var registry = new TypeRegistry();
            registry.LoadAssemblies(dllPaths);
            return registry;
        });
        services.AddSingleton<IMetadataRepository, MetadataRepository>();

        // Register the appropriate formatter based on configuration
        if (useAiFormatter)
        {
            services.AddSingleton<IMcpResponseFormatter, AiConsumptionFormatter>();
        }
        else
        {
            services.AddSingleton<IMcpResponseFormatter, McpResponseFormatter>();
        }

        return services;
    }
}