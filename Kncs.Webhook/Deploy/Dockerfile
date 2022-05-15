# cd ..
# docker build -t jijiechen-docker.pkg.coding.net/sharpcr/kncs/webhook:$(date +"%Y%m%d")-$RANDOM -f ./Deploy/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:7.0.100-preview.4-alpine3.15-amd64 as Builder
WORKDIR /src/
# copy csproj only to reuse nuget cache when csproj is not changing
COPY *.csproj /src/
RUN dotnet restore

COPY . /src/
RUN mkdir /app && dotnet publish -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true -r linux-musl-x64  --self-contained -o /app 




FROM alpine:3.15 
# Install the dependencies according to https://docs.microsoft.com/en-us/dotnet/core/install/linux-alpine#dependencies
RUN apk add bash icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib

WORKDIR /app
COPY --from=Builder /app/* ./

ENTRYPOINT ["/app/Kncs.Webhook"]

