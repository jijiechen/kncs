using k8s;
using Kncs.CrdController.CSharpApp;

namespace Kncs.CrdController;

public class Program
{
    static async Task Main(string[] args)
    {
        var kubeClient = new Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigFile());
        var controller = new CSharpAppController(kubeClient);
        
        var cts = new CancellationTokenSource();
        await controller.StartAsync(cts.Token).ConfigureAwait(false);
        
        // todo: create a valiation webhook!
    }
}