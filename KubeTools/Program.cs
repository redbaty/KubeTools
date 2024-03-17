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
        serviceCollection.AddSingleton(new Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigFile(@"C:\Users\Marcos\.kube\suse")));
        serviceCollection.AddScoped<CleanupFinalizedPods>();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        return await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .UseTypeActivator(serviceProvider.GetRequiredService)
            .Build()
            .RunAsync();
    }
}