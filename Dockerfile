# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY arusha.template.sln .
COPY src/Arusha.Template.Domain/Arusha.Template.Domain.csproj src/Arusha.Template.Domain/
COPY src/Arusha.Template.Application/Arusha.Template.Application.csproj src/Arusha.Template.Application/
COPY src/Arusha.Template.Infrastructure/Arusha.Template.Infrastructure.csproj src/Arusha.Template.Infrastructure/
COPY src/Arusha.Template.Api/Arusha.Template.Api.csproj src/Arusha.Template.Api/

# Restore dependencies (cached if .csproj files haven't changed)
RUN dotnet restore src/Arusha.Template.Api/Arusha.Template.Api.csproj

# Copy everything and build
COPY src/ src/
RUN dotnet publish src/Arusha.Template.Api/Arusha.Template.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

COPY --from=build /app/publish .

# Switch to non-root user
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Arusha.Template.Api.dll"]
