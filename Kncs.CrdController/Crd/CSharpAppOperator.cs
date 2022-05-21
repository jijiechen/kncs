using System.Text;
using Kncs.CrdController.OperatorSDK;
using k8s;
using k8s.Models;
using Newtonsoft.Json;

namespace Kncs.CrdController.Crd;

public class CSharpAppOperator : IOperationHandler<CSharpApp>
{
    private readonly string _watchedNamespace;
    private const string CSharpAppRunnerImage = "jijiechen/csharp-app-runner:2022050301";
    private const string LastApplyConfigAnnoKey = "last-applied-configuration";

    public CSharpAppOperator(string watchedNamespace)
    {
        _watchedNamespace = watchedNamespace;
    }

    public async Task CheckCurrentState(Kubernetes k8s)
    {
        var response = await k8s.ListNamespacedCustomObjectWithHttpMessagesAsync(CSharpApp.SchemaGroup, CSharpApp.SchemaVersion,
            _watchedNamespace, CSharpApp.SchemaKindPlural);

        if (!response.Response.IsSuccessStatusCode)
        {
            return;
        }

        var responseText = await response.Response.Content.ReadAsStringAsync();
        var appListObj = JsonConvert.DeserializeObject<CSharpAppList>(responseText);
        
        foreach (var item in appListObj!.Items)
        {
            await OnUpdated(k8s, item);
        }
    }

    public async Task OnAdded(Kubernetes kubeClient, CSharpApp item)
    {
        if (item.Metadata == null || item.Spec == null)
        {
            return;
        }

        var lastApplied = item.Metadata?.Annotations?[LastApplyConfigAnnoKey];
        if (lastApplied != null)
        {
            await OnUpdated(kubeClient, item);
            return;
        }

        var dummyLastCfg = new LastApplyConfiguration();
        var ownerRef = BuildOwnerReference(item);

        var configmap = EnsureSourceCodeConfigMap(kubeClient, item, ownerRef);

        EnsurePods(kubeClient, item, dummyLastCfg, ownerRef, configmap);

        EnsureService(kubeClient, item, dummyLastCfg, ownerRef);

        var newlyAppliedConfig = new LastApplyConfiguration
        {
            CodeHash = GetCodeHash(item),
            Replicas = item.Spec!.Replicas,
            Service = item.Spec.Service
        };
        var lastApplyCfgAnnoVal = JsonConvert.SerializeObject(newlyAppliedConfig);

        var appPatches = new List<object>();
        if (item.Metadata!.Annotations == null)
        {
            appPatches.Add(new
            {
                op = "add",
                path = $"/metadata/annotations",
                value = new Dictionary<string, object>
                {
                    { LastApplyConfigAnnoKey, lastApplyCfgAnnoVal }
                }
            });

        }
        else
        {
            appPatches.Add(new
            {
                op = "add",
                path = $"/metadata/annotations/{LastApplyConfigAnnoKey}",
                value = lastApplyCfgAnnoVal
            });
        }

        kubeClient.PatchNamespacedCustomObject(
            new V1Patch(JsonConvert.SerializeObject(appPatches), V1Patch.PatchType.JsonPatch),
            CSharpApp.SchemaGroup,
            CSharpApp.SchemaVersion,
            item.Metadata.NamespaceProperty,
            CSharpApp.SchemaKindPlural,
            item.Metadata.Name);
    }


    public async Task OnDeleted(Kubernetes kubeClient, CSharpApp item)
    {
        CleanupService(kubeClient, item);

        CleanupPods(kubeClient, item, null /* delete all pods */);

        CleanupConfigMap(kubeClient, item);
    }


    public async Task OnUpdated(Kubernetes kubeClient, CSharpApp item)
    {
        if (item.Metadata == null || item.Spec == null)
        {
            return;
        }
        
        var ownerRef = BuildOwnerReference(item);

        LastApplyConfiguration? lastAppliedConfig = null;
        var lastApplied = item.Metadata.Annotations?[LastApplyConfigAnnoKey];
        if (lastApplied != null)
        {
            lastAppliedConfig = JsonConvert.DeserializeObject<LastApplyConfiguration>(lastApplied);
        }

        if (lastAppliedConfig == null)
        {
            await OnDeleted(kubeClient, item);
            await OnAdded(kubeClient, item);
            return;
        }

        var codeConfigMap = EnsureSourceCodeConfigMap(kubeClient, item, ownerRef);

        EnsurePods(kubeClient, item, lastAppliedConfig, ownerRef, codeConfigMap);

        EnsureService(kubeClient, item, lastAppliedConfig, ownerRef);

        var codeHash = GetCodeHash(item);
        if (lastAppliedConfig.CodeHash == codeHash)
        {
            return;
        }

        var newlyAppliedConfig = new LastApplyConfiguration
        {
            CodeHash = codeHash,
            Replicas = item.Spec?.Replicas ?? 0,
            Service = item.Spec?.Service
        };
        var appPatches = new List<object>()
        {
            new
            {
                op = "replace",
                path = $"/metadata/annotations/{LastApplyConfigAnnoKey}",
                value = JsonConvert.SerializeObject(newlyAppliedConfig)
            }
        };

        kubeClient.PatchNamespacedCustomObject(new V1Patch( JsonConvert.SerializeObject(appPatches), V1Patch.PatchType.JsonPatch),
            CSharpApp.SchemaGroup,
            CSharpApp.SchemaVersion,
            item.Metadata.NamespaceProperty,
            CSharpApp.SchemaKindPlural,
            item.Metadata.Name);
    }
    
    
    public async Task OnBookmarked(Kubernetes k8s, CSharpApp crd)
    {
        
    }

    public async Task OnError(Kubernetes k8s, CSharpApp crd)
    {
        Console.Error.WriteLine("Some error happens...");
    }
    

    private static V1OwnerReference BuildOwnerReference(CSharpApp item)
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

    private void EnsurePods(Kubernetes kubeClient, CSharpApp item, LastApplyConfiguration lastAppliedConfig,
        V1OwnerReference ownerRef, string codeConfigMap)
    {
        var codeHash = GetCodeHash(item);
        var zeroPods = false;
        if (lastAppliedConfig.CodeHash != null && lastAppliedConfig.CodeHash != codeHash)
        {
            // 源代码已改变，清除旧的 Pod，等待下面的流程来创建
            CleanupPods(kubeClient, item, lastAppliedConfig.CodeHash);
            zeroPods = true;
        }

        var currentPods = Array.Empty<V1Pod>();
        if (!zeroPods)
        {
         
            currentPods = FindPods(kubeClient, item, codeHash)
                .Where(pod => pod.Metadata?.DeletionTimestamp == null).ToArray();
        }
        var offset = item.Spec!.Replicas - currentPods.Length;
        if (offset > 0)
        {
            var podCreated = 0;
            do
            {
                CreatePod(kubeClient, item, codeHash!, ownerRef, codeConfigMap!);
            } while (++podCreated < offset);
        }
        else if (offset < 0)
        {
            // todo: deal with terminating!
            offset = -1 * offset;
            
            var podDeleted = 0;
            do
            {
                kubeClient.DeleteNamespacedPod(currentPods[podDeleted].Metadata.Name, item.Metadata.NamespaceProperty);
            } while (++podDeleted < offset);
        }
    }

    private void EnsureService(Kubernetes kubeClient, CSharpApp item, LastApplyConfiguration lastAppliedConfig,
        V1OwnerReference ownerRef)
    {
        if (item.Spec!.Service == null && lastAppliedConfig.Service != null)
        {
            CleanupService(kubeClient, item);
        }

        var existingSvc = FindService(kubeClient, item);
        if (item.Spec.Service != null && lastAppliedConfig.Service == null && existingSvc == null)
        {
            CreateService(kubeClient, item, ownerRef);
        }

        if (item.Spec.Service != null && lastAppliedConfig.Service != null
                                      && !item.Spec.Service.Equals(lastAppliedConfig.Service))
        {
            if (existingSvc == null)
            {
                CreateService(kubeClient, item, ownerRef);
            }
            else
            {
                var patches = new List<object>();
                if (existingSvc.Spec.Type != item.Spec.Service.Type)
                {
                    patches.Add(new
                    {
                        op = "replace",
                        path = "/spec/type",
                        value = item.Spec.Service.Type
                    });
                }

                existingSvc.Spec.Type = item.Spec.Service.Type;
                var svcPort = existingSvc.Spec.Ports.FirstOrDefault(p => p.Port == item.Spec.Service.Port);
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
                        value = JsonConvert.SerializeObject(ports)
                    });
                }

                kubeClient.PatchNamespacedService(new V1Patch(JsonConvert.SerializeObject(patches), V1Patch.PatchType.JsonPatch), existingSvc.Metadata.Name,
                    existingSvc.Metadata.NamespaceProperty);
            }
        }
    }

    private string EnsureSourceCodeConfigMap(Kubernetes kubeClient, CSharpApp item, V1OwnerReference ownerRef)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        var desiredSourceCode = (item.Spec?.Code ?? "").Trim();
        var existingConfigMap = FindConfigMap(kubeClient, item);
        if (existingConfigMap != null)
        {
            var hasData = existingConfigMap.Data != null;
            var patches = new List<object>();
            if (!hasData)
            {
                patches.Add(new
                {
                    op = "add",
                    path = "/data",
                    value = new Dictionary<string, string>
                    {
                        { "Program.cs", desiredSourceCode }
                    }
                });
            }
            else if (!existingConfigMap.Data!.ContainsKey("Program.cs"))
            {
                patches.Add(
                    new
                    {
                        op = "add",
                        path = "/data/Program.cs",
                        value = desiredSourceCode
                    });
            }
            else
            {
                var source = existingConfigMap.Data["Program.cs"];
                if (source == null || source.Trim() != desiredSourceCode)
                {
                    patches.Add(
                        new
                        {
                            op = "replace",
                            path = "/data/Program.cs",
                            value = desiredSourceCode
                        });
                }
            }

            if (patches.Count > 0)
            {
                var patch = new V1Patch(JsonConvert.SerializeObject(patches), V1Patch.PatchType.JsonPatch);
                kubeClient.PatchNamespacedConfigMap(patch, existingConfigMap.Metadata.Name, item.Metadata.NamespaceProperty);
            }

            return existingConfigMap.Metadata.Name;
        }
        
        
        var cmName = $"csapp-src-{item.Metadata.Name}";
        var configMap = new V1ConfigMap
        {
            ApiVersion = "v1",
            Kind = "ConfigMap",
            Data = new Dictionary<string, string>
            {
                { "Program.cs", desiredSourceCode }
            },
            Metadata = new V1ObjectMeta()
            {
                Labels = new Dictionary<string, string>()
            }
        };
        configMap.AddOwnerReference(ownerRef);

        configMap.Metadata.Name = cmName;
        configMap.Metadata.Labels["csharpapp"] = item.Metadata.Name;
        kubeClient.CreateNamespacedConfigMap(configMap, item.Metadata.NamespaceProperty);
        return cmName;
    }

    #region primitive operations
    
    void CreatePod(Kubernetes kubeClient, CSharpApp item, string codeHash, V1OwnerReference ownerRef, string configmap)
    {
        var shortHash = codeHash.Substring(0, 6);
        var podName = $"csa-{item.Metadata.Name}-{shortHash}-{GenerateRandomName()}";
        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Spec = new V1PodSpec() { Containers = new List<V1Container>(), Volumes = new List<V1Volume>() },
            Metadata = new V1ObjectMeta()
            {
                Labels = new Dictionary<string, string>()
            }
        };

        pod.Metadata.Name = podName;
        pod.Metadata.Labels["csharpapp"] = item.Metadata.Name;
        pod.Metadata.Labels["codehash"] = codeHash;
        pod.AddOwnerReference(ownerRef);

        var cmVolume = new V1Volume("cs-source");
        cmVolume.ConfigMap = new V1ConfigMapVolumeSource()
        {
            Name = configmap
        };
        pod.Spec.Volumes.Add(cmVolume);

        var container = new V1Container("csa")
        {
            Image = CSharpAppRunnerImage,
            ImagePullPolicy = "IfNotPresent",
            VolumeMounts = new List<V1VolumeMount>(),
        };
        container.VolumeMounts.Add(new V1VolumeMount("/etc/cs-source", "cs-source"));
        pod.Spec.Containers.Add(container);

        kubeClient.CreateNamespacedPod(pod, item.Metadata.NamespaceProperty);
    }

    private void CreateService(Kubernetes kubeClient, CSharpApp item, V1OwnerReference ownerRef)
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

    private void CleanupService(Kubernetes kubeClient, CSharpApp item)
    {
        var svc = FindService(kubeClient, item);
        if (svc != null)
        {
            kubeClient.DeleteNamespacedService(svc.Name(), svc.Namespace());
        }
    }

    private void CleanupPods(Kubernetes kubeClient, CSharpApp item, string lastCodeHash)
    {
        var pods = FindPods(kubeClient, item, lastCodeHash).ToArray();

        foreach (var pod in pods)
        {
            kubeClient.DeleteNamespacedPod(pod.Name(), pod.Namespace());
        }
    }

    private void CleanupConfigMap(Kubernetes kubeClient, CSharpApp item)
    {
        var targetConfigMap = FindConfigMap(kubeClient, item);

        if (targetConfigMap != null)
        {
            kubeClient.DeleteNamespacedConfigMap(targetConfigMap.Name(), targetConfigMap.Namespace());
        }
    }

    private V1ConfigMap? FindConfigMap(Kubernetes kubeClient, CSharpApp item)
    {
        var labelSelector = $"csharpapp={item.Metadata.Name}";
        var configMaps = kubeClient.ListNamespacedConfigMap(item.Metadata.NamespaceProperty,
            false, null, null, labelSelector);

        var targetConfigMap = configMaps.Items.FirstOrDefault(cm => cm.OwnerReferences().Any(o
            => o.ApiVersion == item.ApiVersion && o.Kind == item.Kind && o.Name == item.Metadata.Name));
        return targetConfigMap;
    }

    private V1Service? FindService(Kubernetes kubeClient, CSharpApp item)
    {
        var labelSelector = $"csharpapp={item.Metadata.Name}";
        var svcList = kubeClient.ListNamespacedService(item.Metadata.NamespaceProperty,
            false, null, null, labelSelector);

        var targetSvc = svcList.Items.FirstOrDefault(cm => cm.OwnerReferences().Any(o
            => o.ApiVersion == item.ApiVersion && o.Kind == item.Kind && o.Name == item.Metadata.Name));
        return targetSvc;
    }

    private IEnumerable<V1Pod> FindPods(Kubernetes kubeClient, CSharpApp item, string? codeHash)
    {
        var labelSelector = $"csharpapp={item.Metadata.Name}";
        if (codeHash != null)
        {
            labelSelector += $",codehash={codeHash}";
        }
        
        var pods = kubeClient.ListNamespacedPod(item.Metadata.NamespaceProperty,
            false, null, null, labelSelector);

        return pods.Items.Where(cm => cm.OwnerReferences().Any(o
            => o.ApiVersion == item.ApiVersion && o.Kind == item.Kind && o.Name == item.Metadata.Name));
    }
    
    #endregion

    #region helper methods

    static readonly Random NameGeneratorRandom = new();
    private const byte RandomNameLength = 4;

    static string? GetCodeHash(CSharpApp item)
    {
        var code = item.Spec?.Code;
        if (string.IsNullOrEmpty(code))
        {
            return null;
        }

        var hashCode = GetDeterministicHashCode(code);
        return hashCode.ToString("D").Replace('-', 'u');
    }
    
    static int GetDeterministicHashCode(string str)
    {
        unchecked
        {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
    
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
    
    #endregion
}
