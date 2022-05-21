using Kncs.CrdController.OperatorSDK;
using k8s;
using k8s.Models;
using Newtonsoft.Json;

namespace Kncs.CrdController.Crd;

public class CSharpAppSpec
{
    [JsonProperty(PropertyName="code")]
    public string? Code { get; set; }
    
    [JsonProperty(PropertyName="replicas")]
    public byte Replicas { get; set; }
    [JsonProperty(PropertyName="service")]
    public CSharpAppService? Service { get; set; }
}

public class CSharpAppService
{
    [JsonProperty(PropertyName="port")]   
    public short Port { get; set; }
    [JsonProperty(PropertyName="type")]
    public string? Type { get; set; }

    public override bool Equals(object? obj)
    {
        var item = obj as CSharpAppService;

        if (item == null)
        {
            return false;
        }

        return this.Port == item.Port && (bool)(this.Type?.Equals(item.Type));
    }

    // ReSharper disable NonReadonlyMemberInGetHashCode
    public override int GetHashCode()
    {
        if (this.Type == null)
        {
            return this.Port.GetHashCode() ^ "null".GetHashCode();
        }
        
        
        return this.Port.GetHashCode() ^ Type.GetHashCode();
    }
}

public class LastApplyConfiguration
{
    // ReSharper disable once InconsistentNaming
    public string? CodeHash { get; set; }
    public byte Replicas { get; set; }
    public CSharpAppService? Service { get; set; } 
}

public class CSharpApp:  BaseCRD
{
    public const string SchemaGroup = "k8s.jijiechen.com";
    public const string SchemaKindSingular = "csharpapp";
    public const string SchemaKindPlural = "csharpapps";
    public const string SchemaVersion = "v1alpha1";
    
    
    public CSharpApp() : base(SchemaGroup, SchemaVersion, SchemaKindPlural, SchemaKindSingular) { }

    [JsonProperty(PropertyName="spec")]
    public CSharpAppSpec? Spec { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        return ToString().Equals(obj.ToString());
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public override string ToString()
    {
        return Spec?.ToString() ?? string.Empty;
    }

}

  public class CSharpAppList :  IKubernetesObject<V1ListMeta>, IItems<CSharpApp>
  {
    [JsonProperty(PropertyName="apiVersion")]
    public string ApiVersion { get; set; }

    [JsonProperty(PropertyName="items")]
    public IList<CSharpApp> Items { get; set; }

    [JsonProperty(PropertyName="kind")]
    public string Kind { get; set; }

    [JsonProperty(PropertyName="metadata")]
    public V1ListMeta Metadata { get; set; }
  }