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

FROM build AS test
WORKDIR /src
RUN dotnet test "MindSync.Tests/MindSync.Tests.csproj" -c Release --no-restore

FROM test AS publish
WORKDIR "/src/MindSync.Api"
RUN dotnet publish "MindSync.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MindSync.Api.dll"]