# ==============================================================================
# 🚀 FIRMEZA.API - Dockerfile Producción (ASP.NET Core 10 Web API REST)
# Contexto de construcción: Raíz de la solución (.)
# ==============================================================================

# Etapa 1: Compilación y Publicación en Release
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# 1. Copiar archivos de proyecto para restaurar dependencias
COPY Firmeza.slnx ./
COPY src/Firmeza.Domain/*.csproj src/Firmeza.Domain/
COPY src/Firmeza.Application/*.csproj src/Firmeza.Application/
COPY src/Firmeza.Infrastructure/*.csproj src/Firmeza.Infrastructure/
COPY src/Firmeza.Api/*.csproj src/Firmeza.Api/

# 2. Restaurar dependencias
RUN dotnet restore src/Firmeza.Api/Firmeza.Api.csproj

# 3. Copiar código fuente
COPY src/ src/

# 4. Compilar y publicar en modo Release
RUN dotnet publish src/Firmeza.Api/Firmeza.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

# Instalar librerías nativas para QuestPDF y SkiaSharp en Linux
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Firmeza.Api.dll"]
