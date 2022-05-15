FROM mcr.microsoft.com/dotnet/sdk:7.0.100-preview.4-alpine3.15-amd64 as Builder
WORKDIR /src/
# copy csproj only to reuse nuget cache when csproj is not changing
COPY *.csproj /src/
RUN dotnet restore

COPY . /src/
RUN mkdir /app && dotnet publish -c Release -o /app 




FROM mcr.microsoft.com/dotnet/aspnet:7.0.0-preview.4-alpine3.15-amd64 

WORKDIR /app
COPY --from=Builder /app/* ./

ENTRYPOINT ["dotnet", "/app/Kncs.Webhook.dll"]

