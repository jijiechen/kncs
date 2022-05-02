using Kncs.Webhook;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello .NET Kubernetes Webhook!");
app.MapPost("/validate", (AdmissionReview review) => Task.FromResult(Validate(review)));

app.Run();
