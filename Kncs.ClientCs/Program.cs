using k8s;
using k8s.Models;
using Nito.AsyncEx;

namespace Kncs.ClientCs;

public class Program
{
    static async Task Main()
    {
        // 使用凭据构建一个客户端
        var k8sConfig = KubernetesClientConfiguration.BuildDefaultConfig();
        var kubeClient = new Kubernetes(k8sConfig);

        // 找出现有的 Pod
        var pods = await kubeClient.ListNamespacedPodAsync("default", null, null, null, "app=test");
        foreach (var pod in pods.Items)
        {
            Console.WriteLine($"现有的 test Pod: {pod.Metadata.Name}");
        }

        if (pods.Items.Count < 1)
        {
            return;
        }

        // 在现有的 Pod 中执行命令
        foreach (var pod in pods.Items)
        {
            var execResult = await ExecInPod(kubeClient, pod, "hostname -I");
            Console.WriteLine($"Pod {pod.Metadata.Name} 执行结果 {execResult}");
        }
        
        // watch 新的 Pod
        var existingPods = pods.Items.Select(p => p.Metadata.Name).ToHashSet();
        var listTask = await kubeClient.ListNamespacedPodWithHttpMessagesAsync("default", watch: true).ConfigureAwait(false);
        var connectionClosed = new AsyncManualResetEvent();
        
        listTask.Watch<V1Pod, V1PodList>(
            (type, item) =>
            {
                if (type == WatchEventType.Added && !string.IsNullOrEmpty(item?.Metadata.Name) &&
                    existingPods.Contains(item.Metadata.Name))
                {
                    return;
                }
                
                Console.WriteLine($"监视到事件 '{type}'，相关 Pod: {item.Metadata.Name}");
            },
            error =>
            {
                Console.WriteLine($"监视到错误 '{error.GetType().FullName}'");
            },
            connectionClosed.Set);

        Console.WriteLine("等待新的 Pod 事件...");
        await connectionClosed.WaitAsync();
    }

    private static async Task<string> ExecInPod(IKubernetes client, V1Pod pod, string command)
    {
        var webSocket = await client.WebSocketNamespacedPodExecAsync(pod.Metadata.Name, "default", command.Split(" "),
            pod.Spec.Containers[0].Name).ConfigureAwait(false);

        using var demux = new StreamDemuxer(webSocket);
        demux.Start();

        var buff = new byte[4096];
        var stream = demux.GetStream(1, 1);
        var read = stream.Read(buff, 0, 4096);
        return read > 0 ? System.Text.Encoding.Default.GetString(buff, 0, read) : string.Empty;
    }
    
    
    
    
}