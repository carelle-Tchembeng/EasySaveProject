# EasySave.LogServer — Dockerfile
# Multi-stage build: build on SDK image, run on smaller runtime image

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["EasySave.LogServer.csproj", "."]
RUN dotnet restore

# Copy source and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime image ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# /logs is mounted as a Docker volume for persistent log storage
VOLUME ["/logs"]

# Expose HTTP port
EXPOSE 5000

# Log directory environment variable (can be overridden in docker-compose)
ENV LOG_DIR=/logs
ENV ASPNETCORE_URLS=http://+:5000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EasySave.LogServer.dll"]
