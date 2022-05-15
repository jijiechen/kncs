using System.Net;
using System.Text;
using System.Text.Json;
using k8s.Models;
using Kncs.Webhook;

partial class Program
{

    const string InjectAnnotationKeySetting = "k8s.jijiechen.com/inject-dotnet-helper";
    
    static AdmissionReview InjectDotnetHelper(AdmissionReview reviewRequest)
    {
        var allowedResponse = CreateInjectResponse(reviewRequest, true);
        if (reviewRequest.Request.Kind.Kind != "Pod" || reviewRequest.Request.Operation != "CREATE")
        {
            return allowedResponse;
        }

        var podJson = reviewRequest.Request.Object.GetRawText();
        var pod = JsonSerializer.Deserialize<V1Pod>(podJson, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        if (DisabledByAnnotation(pod!.Metadata.Annotations))
        {
            return allowedResponse;
        }
        if (AlreadyInjected(pod))
        {
            return allowedResponse;
        }
        
        var container = new V1Container("dotnet-helper")
        {
            Args = new[] { "infinity" },
            Command = new[] { "sleep" },
            Image = "mcr.microsoft.com/dotnet/runtime:6.0",
            ImagePullPolicy = "IfNotPresent"
        };

        var podName = string.IsNullOrEmpty(reviewRequest.Request.Name) ? pod.Metadata.GenerateName + "?" : reviewRequest.Request.Name;
        var fullPodName = $"{reviewRequest.Request.Namespace}/{podName}";
        Console.WriteLine($"正在向此 Pod 中注入 dotnet helper：{fullPodName}");
        var patch = new
        {
            op = "add",
            path = "/spec/containers/-",
            value = container
        };
        var patches = new[] {patch};
        var patchResponse = new AdmissionResponse()
        {
            Allowed = true,
            PatchType = "JSONPatch",
            Patch = Encoding.Default.GetBytes(JsonSerializer.Serialize(patches))
        };
        var reviewResponse = CreateInjectResponse(reviewRequest, true);
        reviewResponse.Response = patchResponse;
        return reviewResponse;
    }

    private static bool AlreadyInjected(V1Pod pod)
    {
        return pod.Spec.Containers.Any(c => c.Name == "dotnet-helper");
    }

    private static bool DisabledByAnnotation(IDictionary<string, string>? annotations)
    {
        if (annotations == null)
        {
            return false;
        }
        
        var falseValues = new[] { "no", "false", "0" };
            
        return annotations.TryGetValue(InjectAnnotationKeySetting, out var annotation) &&
               falseValues.Contains(annotation.ToLower());
    }

    static AdmissionReview CreateInjectResponse(AdmissionReview originalRequest, bool allowed)
    {
        var res = new AdmissionResponse
        {
            Allowed = allowed,
            Status = new Status
            {
                Code = (int)HttpStatusCode.OK,
                Message = string.Empty
            }
        };
    
        res.Uid = originalRequest.Request.Uid;
        return new AdmissionReview
        {
            ApiVersion = originalRequest.ApiVersion,
            Kind = originalRequest.Kind,
            Response = res
        };
    }
}