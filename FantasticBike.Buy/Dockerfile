FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS publish
WORKDIR /FantasticBike
COPY ["FantasticBike.Buy/FantasticBike.Buy.csproj", "FantasticBike.Buy/"]
COPY ["FantasticBike.Shared/FantasticBike.Shared.csproj", "FantasticBike.Shared/"]
RUN dotnet restore FantasticBike.Buy/FantasticBike.Buy.csproj
COPY ["FantasticBike.Buy/", "FantasticBike.Buy/"]
COPY ["FantasticBike.Shared/", "FantasticBike.Shared/"]
RUN dotnet publish FantasticBike.Buy/FantasticBike.Buy.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/azure-functions/dotnet:3.0 AS runtime
WORKDIR /home/site/wwwroot
EXPOSE 80
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true