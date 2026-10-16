FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MindSync.Api/MindSync.Api.csproj", "MindSync.Api/"]
COPY ["MindSync.Application/MindSync.Application.csproj", "MindSync.Application/"]
COPY ["MindSync.Domain/MindSync.Domain.csproj", "MindSync.Domain/"]
COPY ["MindSync.Infrastructure/MindSync.Infrastructure.csproj", "MindSync.Infrastructure/"]
COPY ["MindSync.Tests/MindSync.Tests.csproj", "MindSync.Tests/"]

RUN dotnet restore "MindSync.Api/MindSync.Api.csproj"
RUN dotnet restore "MindSync.Tests/MindSync.Tests.csproj"
COPY . .

WORKDIR "/src/MindSync.Api"
RUN dotnet build "MindSync.Api.csproj" -c Release -o /app/build

# Quality gate: runs the UNIT test suite during the image build (chained into
# publish below, so it's part of the default build graph and a failing test
# fails "docker build"). IntegrationTests are excluded on purpose: they boot
# the real Program.cs via WebApplicationFactory, which needs a live SQL Server
# reachable — something that doesn't exist yet at "docker build" time, before
# docker-compose has created the network and started the sqlserver service.
# Run integration tests separately, against the stack once it's already up
# (e.g. "dotnet test --filter FullyQualifiedName~IntegrationTests" locally).
FROM build AS test
WORKDIR /src
RUN dotnet test "MindSync.Tests/MindSync.Tests.csproj" -c Release --no-restore \
    --filter "FullyQualifiedName!~IntegrationTests"

FROM test AS publish
WORKDIR "/src/MindSync.Api"
RUN dotnet publish "MindSync.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MindSync.Api.dll"]