using CliFx;
using k8s;
using KubeTools.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace KubeTools;

public static class Program
{
    public static async Task<int> Main()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(
            #if RELEASE
            new Kubernetes(KubernetesClientConfiguration.InClusterConfig())
#else
            new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig())
#endif
        );
        serviceCollection.AddScoped<CleanupFinalizedPods>();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        return await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .UseTypeActivator(serviceProvider.GetRequiredService)
            .Build()
            .RunAsync();
    }
}