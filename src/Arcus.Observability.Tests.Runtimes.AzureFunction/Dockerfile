#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:4.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0.100-bullseye-slim AS build
WORKDIR /src
COPY ["Arcus.Observability.Tests.Runtimes.AzureFunction/Arcus.Observability.Tests.Runtimes.AzureFunction.csproj", "Arcus.Observability.Tests.Runtimes.AzureFunction/"]
RUN dotnet restore "Arcus.Observability.Tests.Runtimes.AzureFunction/Arcus.Observability.Tests.Runtimes.AzureFunction.csproj"
COPY . .
WORKDIR "/src/Arcus.Observability.Tests.Runtimes.AzureFunction"
RUN dotnet build "Arcus.Observability.Tests.Runtimes.AzureFunction.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Arcus.Observability.Tests.Runtimes.AzureFunction.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true