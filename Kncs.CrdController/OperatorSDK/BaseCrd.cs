// Copied from https://github.com/ContainerSolutions/dotnet-operator-sdk
// Json properties added

using System.Text.Json.Serialization;
using k8s;
using k8s.Models;
using Newtonsoft.Json;

namespace Kncs.CrdController.OperatorSDK;

public abstract class BaseCRD : IMetadata<V1ObjectMeta>
{
    protected BaseCRD(string group, string version, string plural, string singular, int reconInterval = 5)
    {
        Group = group;
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Plural = plural;
        Singular = singular;
        ReconciliationCheckInterval = reconInterval;
    }

    public int ReconciliationCheckInterval { get; protected set; }
    public string Group { get; protected set; }
    public string Version { get; protected set; }
    public string Plural { get; protected set; }
    public string Singular { get; protected set; }
    public string StatusAnnotationName { get => string.Format($"{Group}/{Singular}-status"); }

    [JsonProperty(PropertyName="status")]
    public string? Status => Metadata.Annotations == null ? null : 
        (Metadata.Annotations.ContainsKey(StatusAnnotationName) ? Metadata.Annotations[StatusAnnotationName] : null);
    [JsonProperty(PropertyName="apiVersion")]
    public string ApiVersion { get; set; } = null!;

    [JsonProperty(PropertyName="kind")]
    public string Kind { get; set; } = null!;

    [JsonProperty(PropertyName="metadata")]
    public V1ObjectMeta Metadata { get; set; } = null!;
}