FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/QuickMind.Api/QuickMind.Api.csproj", "src/QuickMind.Api/"]
COPY ["src/QuickMind.Infrastructure/QuickMind.Infrastructure.csproj", "src/QuickMind.Infrastructure/"]
COPY ["src/QuickMind.Application/QuickMind.Application.csproj", "src/QuickMind.Application/"]
COPY ["src/QuickMind.Domain/QuickMind.Domain.csproj", "src/QuickMind.Domain/"]
RUN dotnet restore "src/QuickMind.Api/QuickMind.Api.csproj"
COPY . .
WORKDIR "/src/src/QuickMind.Api"
RUN dotnet build "QuickMind.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "QuickMind.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuickMind.Api.dll"]
