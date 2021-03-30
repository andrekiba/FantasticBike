FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS publish
WORKDIR /FantasticBike
COPY ["FantasticBike.Shipper/FantasticBike.Shipper.csproj", "FantasticBike.Shipper/"]
COPY ["FantasticBike.Shared/FantasticBike.Shared.csproj", "FantasticBike.Shared/"]
RUN dotnet restore FantasticBike.Shipper/FantasticBike.Shipper.csproj
COPY ["FantasticBike.Shipper/", "FantasticBike.Shipper/"]
COPY ["FantasticBike.Shared/", "FantasticBike.Shared/"]
RUN dotnet publish FantasticBike.Shipper/FantasticBike.Shipper.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/azure-functions/dotnet:3.0 AS runtime
WORKDIR /home/site/wwwroot
EXPOSE 80
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true