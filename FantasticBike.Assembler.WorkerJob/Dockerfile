FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["FantasticBike.Assembler.WorkerJob/FantasticBike.Assembler.WorkerJob.csproj", "FantasticBike.Assembler.WorkerJob/"]
RUN dotnet restore "FantasticBike.Assembler.WorkerJob/FantasticBike.Assembler.WorkerJob.csproj"
COPY . .
WORKDIR "/src/FantasticBike.Assembler.WorkerJob"
RUN dotnet build "FantasticBike.Assembler.WorkerJob.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FantasticBike.Assembler.WorkerJob.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FantasticBike.Assembler.WorkerJob.dll"]
