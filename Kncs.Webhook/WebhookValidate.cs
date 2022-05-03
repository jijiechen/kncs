using System.Net;
using System.Text.Json;
using k8s.Models;
using Kncs.Webhook;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

partial class Program
{
    private static string[]? _blockedRepos = null;

    static AdmissionReview CheckImage(AdmissionReview reviewRequest)
    {
        if (_blockedRepos == null)
        {
            _blockedRepos = (Environment.GetEnvironmentVariable("BLOCKED_REPOS") ?? "").Split(",");
        }

        var allowed = true;
        if (reviewRequest.Request.Kind.Kind != "Pod" || reviewRequest.Request.Operation != "CREATE")
        {
            return CreateReviewResponse(reviewRequest, allowed);
        }

        var pod = JsonSerializer.Deserialize<V1Pod>(reviewRequest.Request.Object.GetRawText(),
            new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        if (pod == null)
        {
            return CreateReviewResponse(reviewRequest, allowed);
        }

        var podName = string.IsNullOrEmpty(reviewRequest.Request.Name) ? pod.Metadata.GenerateName + "?" : reviewRequest.Request.Name;
        var fullPodName = $"{reviewRequest.Request.Namespace}/{podName}";
        var usedImages = new List<string>()
            .Concat(pod.Spec.Containers.NotEmpty().Select(c => c.Image))
            .Concat(pod.Spec.InitContainers.NotEmpty().Select(c => c.Image))
            .Distinct()
            .ToList();

        var blockedImages = new List<string>();
        var blockedCount = usedImages.Select(img =>
        {
            var shouldBlock = ImageBlocked(img);
            if (shouldBlock)
            {
                blockedImages.Add(img);
                Console.WriteLine($"Pod {fullPodName} 被拦截，因为使用被禁止的镜像 {img}");
            }

            return shouldBlock;
        }).Count(b => b);

        allowed = blockedCount == 0;
        var res = CreateReviewResponse(reviewRequest, allowed);
        if (!allowed)
        {
            var imageList = string.Join(",", blockedImages);
            res.Response.Status.Message = $"dotnet webhook: Pod should not use these images: {imageList}";
        }

        return res;
    }

    static bool ImageBlocked(string imageLocation)
    {
        if (_blockedRepos!.Length == 1 && string.IsNullOrWhiteSpace(_blockedRepos[0]))
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