FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/services/BoardWrite/Tasker.BoardWrite.Api/Tasker.BoardWrite.Api.csproj
RUN dotnet publish src/services/BoardWrite/Tasker.BoardWrite.Api/Tasker.BoardWrite.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 DOTNET_RUNNING_IN_CONTAINER=true
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Tasker.BoardWrite.Api.dll"]
