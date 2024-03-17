using System.Text;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using k8s;

namespace KubeTools.Commands;

[Command("cleanup-finalized-pods", Description = "Delete all succeeded pods")]
public class CleanupFinalizedPods : ICommand
{
    private readonly Kubernetes _kubectl;

    [CommandOption("min-age", 'm', Description = "Minimum age of pods to delete", EnvironmentVariable = "MIN_AGE")]
    public TimeSpan MinAge { get; set; } = TimeSpan.FromHours(1);

    [CommandOption("cleanup-failed", 'f', Description = "Delete failed pods", EnvironmentVariable = "CLEANUP_FAILED")]
    public bool CleanupFailed { get; set; } = true;
    
    [CommandOption("cleanup-evicted", 'e', Description = "Delete evicted pods", EnvironmentVariable = "CLEANUP_EVICTED")]
    public bool CleanupEvicted { get; set; } = true;

    public CleanupFinalizedPods(Kubernetes kubectl)
    {
        _kubectl = kubectl;
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var fieldSelectorStringBuilder = new StringBuilder("status.phase=Succeeded");

        if (CleanupFailed)
        {
            fieldSelectorStringBuilder.Append(",status.phase=Failed");
        }
        
        if (CleanupEvicted)
        {
            fieldSelectorStringBuilder.Append(",status.phase=Evicted");
        }

        var pods = await _kubectl.ListPodForAllNamespacesAsync(fieldSelector: fieldSelectorStringBuilder.ToString());
        foreach (var pod in pods.Items)
        {
            if (pod.Status.Phase == "Succeeded")
            {
                var nonReadyTime = pod.Status.Conditions
                    .Where(c => c.Type == "Ready" && c.Status != "True")
                    .Select(i => i.LastTransitionTime)
                    .Where(i => i != null)
                    .Max();

                var timeDelta = DateTimeOffset.Now - nonReadyTime;

                if (timeDelta > MinAge)
                {
                    await _kubectl.DeleteNamespacedPodAsync(pod.Metadata.Name, pod.Metadata.NamespaceProperty);
                    await console.Output.WriteLineAsync($"Deleted pod {pod.Metadata.Name}");
                }
            }
            else
            {
                await _kubectl.DeleteNamespacedPodAsync(pod.Metadata.Name, pod.Metadata.NamespaceProperty);
                await console.Output.WriteLineAsync($"Deleted pod {pod.Metadata.Name}");
            }
        }
        
        if(pods.Items.Count == 0)
        {
            await console.Output.WriteLineAsync("No pods to delete");
        }
        
        await console.Output.WriteLineAsync("Done");
    }
}