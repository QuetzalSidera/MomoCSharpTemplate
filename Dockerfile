FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["templates/service/ServiceTemplate.csproj", "templates/service/"]
COPY ["packages/dotnet/shared/Shared.csproj", "packages/dotnet/shared/"]
RUN dotnet restore "templates/service/ServiceTemplate.csproj"
COPY . .
WORKDIR "/src/templates/service"
RUN dotnet build "./ServiceTemplate.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ServiceTemplate.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServiceTemplate.dll"]
