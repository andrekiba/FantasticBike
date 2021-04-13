FROM mcr.microsoft.com/dotnet/sdk:5.0 AS publish
WORKDIR /FantasticBike
COPY ["FantasticBike.Assembler.WorkerJob/FantasticBike.Assembler.WorkerJob.csproj", "FantasticBike.Assembler.WorkerJob/"]
COPY ["FantasticBike.Shared/FantasticBike.Shared.csproj", "FantasticBike.Shared/"]
RUN dotnet restore FantasticBike.Assembler.WorkerJob/FantasticBike.Assembler.WorkerJob.csproj
COPY ["FantasticBike.Assembler.WorkerJob/", "FantasticBike.Assembler.WorkerJob/"]
COPY ["FantasticBike.Shared/", "FantasticBike.Shared/"]
RUN dotnet publish FantasticBike.Assembler.WorkerJob/FantasticBike.Assembler.WorkerJob.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FantasticBike.Assembler.WorkerJob.dll"]
