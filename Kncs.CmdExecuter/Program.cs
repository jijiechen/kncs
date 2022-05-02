
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using k8s;
using k8s.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Kncs.CmdExecuter;

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("已安装的 kubectl 版本: ");
        Console.WriteLine(ExecuteKubectl("version", null));
        
        var random = new Random().Next(10000, 99999);
        var podName = $"testpod-{random}";

        var podYaml = GetPodYaml(podName);
        if (args.Length > 0)
        {
            podName = $"csharp-{podName}";
            podYaml = ComposePodAsYaml(podName);
        }
        Console.WriteLine($"准备创建 Pod {podName}：");
        Console.WriteLine(podYaml);
        
        var createOutput = ExecuteKubectl("-n default create -f -", podYaml);
        Console.WriteLine("创建 Pod 的结果：");
        Console.WriteLine(createOutput);

        var getOutput = ExecuteKubectl($"-n default get pods {podName}", null);
        Console.WriteLine("获取 Pod 的结果：");
        Console.WriteLine(getOutput);
    }

    static string ExecuteKubectl(string args, string? stdin)
    {
        var kubectl = new ProcessStartInfo("kubectl", args)
        {
            RedirectStandardOutput = true
        };
        var kubectlTask = ProcessAsyncHelper.RunAsync(kubectl, stdin);
        kubectlTask.ConfigureAwait(false).GetAwaiter().GetResult();
        return kubectlTask.Result.StdOut;
    }

    static string? GetPodYaml(string podName)
    {
        using var resStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Kncs.CmdExecuter.Manifests.pod.yaml");
        if (resStream == null) return string.Empty;

        using var reader = new StreamReader(resStream);
        var podYamlTemplate = reader.ReadToEnd();

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance) 
            .Build();

        dynamic p = deserializer.Deserialize<ExpandoObject>(podYamlTemplate);
        p.metadata["name"] = podName;
        
        var serializer = new SerializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .Build();

        var writer = new StringWriter();
        serializer.Serialize(writer, p);

        return writer.ToString();
    }
    
    
    static string ComposePodAsYaml(string podName)
    {
        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Spec = new V1PodSpec(){Containers = new List<V1Container>()},
            Metadata = new V1ObjectMeta(){Labels = new Dictionary<string, string>()}
        };

        pod.Metadata.Name = podName;
        pod.Metadata.Labels["app"] = "test";
        
        var container = new V1Container("tester")
        {
            Args = new[] { "infinity" },
            Command = new[] { "sleep" },
            Image = "centos:7",
            ImagePullPolicy = "IfNotPresent"
        };
        pod.Spec.Containers.Add(container);

        return KubernetesYaml.Serialize(pod);
    }

}