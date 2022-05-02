using System.Net;
using System.Text.Json;
using k8s.Models;
using Kncs.Webhook;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;


partial class Program
{
    private static string[]? _blockedRepos = null;

    static AdmissionReview Validate(AdmissionReview review)
    {
        if (_blockedRepos == null)
        {
            _blockedRepos = (Environment.GetEnvironmentVariable("BLOCKED_REPOS") ?? "").Split(",");
        }

        var podJson = review.Request.Object.GetRawText();
        var pod = JsonSerializer.Deserialize<V1Pod>(podJson,
            new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var allowed = true;
        if (pod?.Spec != null)
        {
            var podName = $"{pod.Metadata.NamespaceProperty}/{pod.Metadata.Name}";
            var usedImages = new List<string>()
                .Concat(pod.Spec.Containers.NotEmpty().Select(c => c.Image))
                .Concat(pod.Spec.InitContainers.NotEmpty().Select(c => c.Image))
                .Concat(pod.Spec.EphemeralContainers.NotEmpty().Select(c => c.Image))
                .Distinct()
                .ToList();

            var blockedCount = usedImages.Select(img =>
            {
                var shouldBlock = ImageBlocked(img);
                if (!shouldBlock)
                {
                    Console.WriteLine($"Pod {podName} 被拦截，因为使用被禁止的镜像 {img}");
                }

                return shouldBlock;
            }).Count();

            allowed = blockedCount == 0;
        }

        return CreateReviewResponse(review, allowed);
    }

    static bool ImageBlocked(string imageLocation)
    {
        if (_blockedRepos!.Length == 0 && string.IsNullOrWhiteSpace(_blockedRepos[0]))
        {
            return false;
        }
    
        if (!imageLocation.Contains('/'))
        {
            imageLocation = "docker.io/" + imageLocation;
        }

        return _blockedRepos
            .Select(r => r.EndsWith('/') ? r : string.Concat(r, '/'))
            .Any(r => imageLocation.StartsWith(r));
    }

    static AdmissionReview CreateReviewResponse(AdmissionReview originalRequest, bool allowed)
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