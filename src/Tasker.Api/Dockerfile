# Stage 1 — build & publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем решение и проекты (оптимизация layer'ов restore)
COPY ["Tasker.sln", "./"]
COPY ["src/Tasker.Api/Tasker.Api.csproj", "src/Tasker.Api/"]
COPY ["src/Tasker.Data/Tasker.Data.csproj", "src/Tasker.Data/"]
COPY ["src/Tasker.Application/Tasker.Application.csproj", "src/Tasker.Application/"]
COPY ["src/Tasker.Core/Tasker.Core.csproj", "src/Tasker.Core/"]

RUN dotnet restore "src/Tasker.Api/Tasker.Api.csproj"

# Копируем всё и публикуем
COPY . .
WORKDIR /src/src/Tasker.Api
RUN dotnet publish "Tasker.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2 — runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Tasker.Api.dll"]
