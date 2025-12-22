FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/services/AnalyticsIngest/Tasker.AnalyticsIngest.Worker/Tasker.AnalyticsIngest.Worker.csproj
RUN dotnet publish src/services/AnalyticsIngest/Tasker.AnalyticsIngest.Worker/Tasker.AnalyticsIngest.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
ENV DOTNET_RUNNING_IN_CONTAINER=true
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Tasker.AnalyticsIngest.Worker.dll"]
