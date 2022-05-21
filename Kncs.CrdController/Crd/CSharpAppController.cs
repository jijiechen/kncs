using System.Text;
using System.Text.Json;
using ContainerSolutions.OperatorSDK;
using k8s;
using k8s.Models;

namespace Kncs.CrdController.Crd;

public class CSharpAppController : IOperationHandler<Crd.CSharpApp>
{
    private const string CSharpAppRunnerImage = "jijiechen/csharp-app-runner:2022050301";
    private const string LastApplyConfigAnnoKey = "last-applied-configuration";


    public async Task OnBookmarked(Kubernetes k8s, Crd.CSharpApp crd)
    {
        
    }

    public async Task OnError(Kubernetes k8s, Crd.CSharpApp crd)
    {
        Console.Error.WriteLine("Some error happens...");
    }

    public async Task CheckCurrentState(Kubernetes k8s)
    {
        
    }
    
    public async Task OnAdded(Kubernetes kubeClient, Crd.CSharpApp item)
    {
        var ownerRef = BuildOwnerReference(item);

        var configmap = CreateConfigMap(kubeClient, item, ownerRef);
        var podCreated = 0;
        while (podCreated++ < item.Spec.Replicas)
        {
            CreatePod(kubeClient, item, ownerRef, configmap);
        }

        if (item.Spec.Service != null)
        {
            CreateService(kubeClient, item, ownerRef);
        }

        // todo: create watcher!
        var newlyAppliedConfig = new LastApplyConfiguration
        {
            CodeHash = item.Spec.Code!.GetHashCode(),
            Replicas = item.Spec.Replicas,
            Service = item.Spec.Service
        };
        var appPatches = new List<object>()
        {
            new
            {
                op = "add",
                path = $"/metadata/annotations/{LastApplyConfigAnnoKey}",
                value = JsonSerializer.Serialize(newlyAppliedConfig)
            }
        };

        kubeClient.PatchNamespacedCustomObject(new V1Patch { Content = appPatches },
            Crd.CSharpApp.SchemaGroup,
            Crd.CSharpApp.SchemaVersion,
            item.Metadata.NamespaceProperty,
            Crd.CSharpApp.SchemaKindPlural,
            item.Metadata.Name);
    }

    private static V1OwnerReference BuildOwnerReference(Crd.CSharpApp item)
    {
        var ownerRef = new V1OwnerReference()
        {
            Kind = item.Kind,
            ApiVersion = item.ApiVersion,
            Name = item.Metadata.Name,
            Uid = item.Metadata.Uid
        };
        return ownerRef;
    }

    
    public async Task OnDeleted(Kubernetes kubeClient, Crd.CSharpApp item)
    {
        CleanupService(kubeClient, item);

        CleanupPods(kubeClient, item);

        CleanupConfigMap(kubeClient, item);
    }

    
    public async Task OnUpdated(Kubernetes kubeClient, Crd.CSharpApp item)
    {
        var ownerRef = BuildOwnerReference(item);

        LastApplyConfiguration? lastAppliedConfig = null;
        var lastApplied = item.Metadata.Annotations[LastApplyConfigAnnoKey];
        if (lastApplied != null)
        {
            lastAppliedConfig = JsonSerializer.Deserialize<LastApplyConfiguration>(lastApplied);
        }

        if (lastAppliedConfig == null)
        {
            await OnDeleted(kubeClient, item);
            await OnAdded(kubeClient, item);
            return;
        }

        var srcConfigMap = FindConfigMap(kubeClient, item)?.Metadata.Name;
        var newlyCreatedCm = srcConfigMap == null;
        if (newlyCreatedCm)
        {
            srcConfigMap = CreateConfigMap(kubeClient, item, ownerRef);
        }

        var codeHash = item.Spec.Code!.GetHashCode();
        if (!newlyCreatedCm && codeHash != lastAppliedConfig.CodeHash)
        {
            var patches = new List<object>
            {
                new
                {
                    op = "replace",
                    path = "/data/Program.cs",
                    value = item.Spec.Code
                }
            };

            var patch = new V1Patch { Content = patches };
            kubeClient.PatchNamespacedConfigMap(patch, srcConfigMap, item.Metadata.NamespaceProperty);

            CleanupPods(kubeClient, item);
            lastAppliedConfig.Replicas = 0;
        }

        var offset = item.Spec.Replicas - lastAppliedConfig.Replicas;
        if (offset > 0)
        {
            var podCreated = 0;
            do
            {
                CreatePod(kubeClient, item, ownerRef, srcConfigMap!);
            } while (++podCreated < offset);
        }
        else if (offset < 0)
        {
            offset = -1 * offset;
            var pods = FindPods(kubeClient, item).ToArray();
            var podDeleted = 0;
            do
            {
                kubeClient.DeleteNamespacedPod(pods[podDeleted].Metadata.Name, item.Metadata.NamespaceProperty);
            } while (++podDeleted < offset);
        }

        if (item.Spec.Service == null && lastAppliedConfig.Service != null)
        {
            CleanupService(kubeClient, item);
        }

        if (item.Spec.Service != null && lastAppliedConfig.Service == null)
        {
            CreateService(kubeClient, item, ownerRef);
        }

        if (item.Spec.Service != null && lastAppliedConfig.Service != null
                                      && !item.Spec.Service.Equals(lastAppliedConfig.Service))
        {
            var svc = FindService(kubeClient, item);
            if (svc == null)
            {
                CreateService(kubeClient, item, ownerRef);
            }
            else
            {
                var patches = new List<object>();
                if (svc.Spec.Type != item.Spec.Service.Type)
                {
                    patches.Add(new
                    {
                        op = "replace",
                        path = "/spec/type",
                        value = item.Spec.Service.Type
                    });
                }

                svc.Spec.Type = item.Spec.Service.Type;
                var svcPort = svc.Spec.Ports.FirstOrDefault(p => p.Port == item.Spec.Service.Port);
                if (svcPort == null)
                {
                    var ports = new List<V1ServicePort>()
                    {
                        new() { Port = item.Spec.Service.Port, TargetPort = item.Spec.Service.Port }
                    };
                    patches.Add(new
                    {
                        op = "replace",
                        path = "/spec/ports",
                        value = JsonSerializer.Serialize(ports)
                    });
                }

                kubeClient.PatchNamespacedService(new V1Patch { Content = patches }, svc.Metadata.Name,
                    svc.Metadata.NamespaceProperty);
            }
        }

        var newlyAppliedConfig = new LastApplyConfiguration
        {
            CodeHash = item.Spec.Code.GetHashCode(),
            Replicas = item.Spec.Replicas,
            Service = item.Spec.Service
        };
        var appPatches = new List<object>()
        {
            new
            {
                op = "replace",
                path = $"/metadata/annotations/{LastApplyConfigAnnoKey}",
                value = JsonSerializer.Serialize(newlyAppliedConfig)
            }
        };

        kubeClient.PatchNamespacedCustomObject(new V1Patch { Content = appPatches },
            Crd.CSharpApp.SchemaGroup,
            Crd.CSharpApp.SchemaVersion,
            item.Metadata.NamespaceProperty,
            Crd.CSharpApp.SchemaKindPlural,
            item.Metadata.Name);
    }

    private string CreateConfigMap(Kubernetes kubeClient, Crd.CSharpApp item, V1OwnerReference ownerRef)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (item == null) throw new ArgumentNullException(nameof(item));
        var cmName = $"csapp-src-{item.Metadata.Name}";
        var configMap = new V1ConfigMap
        {
            ApiVersion = "v1",
            Kind = "ConfigMap",
            Data = new Dictionary<string, string>
            {
                { "Program.cs", item.Spec.Code ?? "" }
            }
        };
        configMap.AddOwnerReference(ownerRef);

        configMap.Metadata.Name = cmName;
        configMap.Metadata.Labels["csharpapp"] = item.Metadata.Name;
        kubeClient.CreateNamespacedConfigMap(configMap, item.Metadata.NamespaceProperty);
        return cmName;
    }

    void CreatePod(Kubernetes kubeClient, Crd.CSharpApp item, V1OwnerReference ownerRef, string configmap)
    {
        var podName = $"csapp-{item.Metadata.Name}-{GenerateRandomName()}";
        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Spec = new V1PodSpec() { Containers = new List<V1Container>() },
            Metadata = new V1ObjectMeta()
            {
                Labels = new Dictionary<string, string>()
            }
        };

        pod.Metadata.Name = podName;
        pod.Metadata.Labels["csharpapp"] = item.Metadata.Name;
        pod.AddOwnerReference(ownerRef);

        var cmVolume = new V1Volume("cs-source");
        cmVolume.ConfigMap = new V1ConfigMapVolumeSource()
        {
            Name = configmap
        };
        pod.Spec.Volumes.Add(cmVolume);

        var container = new V1Container("tester")
        {
            Image = CSharpAppRunnerImage,
            ImagePullPolicy = "IfNotPresent",
            VolumeMounts = new List<V1VolumeMount>(),
        };
        container.VolumeMounts.Add(new V1VolumeMount("/etc/cs-source", "cs-source"));
        pod.Spec.Containers.Add(container);

        kubeClient.CreateNamespacedPod(pod, item.Metadata.NamespaceProperty);
    }

    private void CreateService(Kubernetes kubeClient, Crd.CSharpApp item, V1OwnerReference ownerRef)
    {
        var svc = new V1Service()
        {
            ApiVersion = "v1",
            Kind = "Service",
            Spec = new V1ServiceSpec()
            {
                Ports = new List<V1ServicePort>()
            },
            Metadata = new V1ObjectMeta()
            {
                Labels = new Dictionary<string, string>()
            }
        };

        svc.Metadata.Name = item.Metadata.Name;
        svc.Metadata.Labels["csharpapp"] = item.Metadata.Name;
        svc.Spec.Type = item.Spec.Service!.Type;
        svc.Spec.Ports.Add(new V1ServicePort()
        {
            Port = item.Spec.Service.Port,
            TargetPort = item.Spec.Service.Port
        });
        svc.Spec.Selector = new Dictionary<string, string>()
        {
            { "csharpapp", item.Metadata.Name }
        };
        svc.AddOwnerReference(ownerRef);

        kubeClient.CreateNamespacedService(svc, item.Metadata.NamespaceProperty);
    }

    private void CleanupService(Kubernetes kubeClient, Crd.CSharpApp item)
    {
        var svc = FindService(kubeClient, item);
        if (svc != null)
        {
            kubeClient.DeleteNamespacedService(svc.Name(), svc.Namespace());
        }
    }

    private void CleanupPods(Kubernetes kubeClient, Crd.CSharpApp item)
    {
        var pods = FindPods(kubeClient, item).ToArray();

        foreach (var pod in pods)
        {
            kubeClient.DeleteNamespacedPod(pod.Name(), pod.Namespace());
        }
    }

    private void CleanupConfigMap(Kubernetes kubeClient, Crd.CSharpApp item)
    {
        var targetConfigMap = FindConfigMap(kubeClient, item);

        if (targetConfigMap != null)
        {
            kubeClient.DeleteNamespacedConfigMap(targetConfigMap.Name(), targetConfigMap.Namespace());
        }
    }

    private V1ConfigMap? FindConfigMap(Kubernetes kubeClient, Crd.CSharpApp item)
    {
        var labelSelector = $"csharpapp={item.Metadata.Name}";
        var configMaps = kubeClient.ListNamespacedConfigMap(item.Metadata.NamespaceProperty,
            false, null, null, labelSelector);

        var targetConfigMap = configMaps.Items.FirstOrDefault(cm => cm.OwnerReferences().Any(o
            => o.ApiVersion == item.ApiVersion && o.Kind == item.Kind && o.Name == item.Metadata.Name));
        return targetConfigMap;
    }

    private V1Service? FindService(Kubernetes kubeClient, Crd.CSharpApp item)
    {
        var labelSelector = $"csharpapp={item.Metadata.Name}";
        var svcList = kubeClient.ListNamespacedService(item.Metadata.NamespaceProperty,
            false, null, null, labelSelector);

        var targetSvc = svcList.Items.FirstOrDefault(cm => cm.OwnerReferences().Any(o
            => o.ApiVersion == item.ApiVersion && o.Kind == item.Kind && o.Name == item.Metadata.Name));
        return targetSvc;
    }

    private IEnumerable<V1Pod> FindPods(Kubernetes kubeClient, Crd.CSharpApp item)
    {
        var labelSelector = $"csharpapp={item.Metadata.Name}";
        var pods = kubeClient.ListNamespacedPod(item.Metadata.NamespaceProperty,
            false, null, null, labelSelector);

        return pods.Items.Where(cm => cm.OwnerReferences().Any(o
            => o.ApiVersion == item.ApiVersion && o.Kind == item.Kind && o.Name == item.Metadata.Name));
    }

    static readonly Random NameGeneratorRandom = new();
    private const byte RandomNameLength = 4;

    static string GenerateRandomName()
    {
        var stringBuilder = new StringBuilder();
        while (stringBuilder.Length < RandomNameLength)
        {
            var letter = NameGeneratorRandom.Next(97, 122);
            stringBuilder.Append((char)letter);
        }

        return stringBuilder.ToString();
    }
}
