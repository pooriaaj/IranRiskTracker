FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["IranRiskTracker.Api/IranRiskTracker.Api.csproj", "IranRiskTracker.Api/"]
COPY ["IranRiskTracker.Application/IranRiskTracker.Application.csproj", "IranRiskTracker.Application/"]
COPY ["IranRiskTracker.Domain/IranRiskTracker.Domain.csproj", "IranRiskTracker.Domain/"]
COPY ["IranRiskTracker.Infrastructure/IranRiskTracker.Infrastructure.csproj", "IranRiskTracker.Infrastructure/"]
RUN dotnet restore "IranRiskTracker.Api/IranRiskTracker.Api.csproj"

COPY . .
RUN dotnet publish "IranRiskTracker.Api/IranRiskTracker.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "IranRiskTracker.Api.dll"]
