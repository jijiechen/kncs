using Kncs.Webhook;
using Microsoft.AspNetCore.HttpLogging;

var debugEnabled = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEBUG"));
var builder = WebApplication.CreateBuilder(args);
if (debugEnabled)
{
    builder.Services.AddHttpLogging(options =>
    {
        options.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders |
                                HttpLoggingFields.ResponsePropertiesAndHeaders |
                                HttpLoggingFields.RequestBody | HttpLoggingFields.ResponseBody;
    });
}


var app = builder.Build();
if (debugEnabled)
{
    app.UseHttpLogging();
}

app.MapGet("/", () => "Hello .NET Kubernetes Webhook!");
app.MapPost("/validate", (AdmissionReview review) => Task.FromResult(CheckImage(review)));
app.MapPost("/mutate", (AdmissionReview review) => Task.FromResult(InjectDotnetHelper(review)));

app.Run();
