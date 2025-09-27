using McpNetDll.Helpers;
using McpNetDll.Registry;
using McpNetDll.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace McpNetDll.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, string[] dllPaths)
    {
        services.AddSingleton<ITypeRegistry>(sp =>
        {
            var registry = new TypeRegistry();
            registry.LoadAssemblies(dllPaths);
            return registry;
        });
        services.AddSingleton<IMetadataRepository, MetadataRepository>();
        services.AddSingleton<IMcpResponseFormatter, McpResponseFormatter>();
        return services;
    }
}