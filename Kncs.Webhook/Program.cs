using Kncs.Webhook;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello .NET Kubernetes Webhook!");
app.MapPost("/validate", (AdmissionReview review) => Task.FromResult(CheckImage(review)));
app.MapPost("/mutate", (AdmissionReview review) => Task.FromResult(InjectDotnetHelper(review)));

app.Run();
