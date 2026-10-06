# ==============================================================================
# 🏗️ FIRMEZA - Dockerfile Multi-Stage (Frontend Angular + Backend .NET 10)
# ==============================================================================

# Etapa 1: Compilación de Frontend Angular SPA
FROM node:20-alpine AS build-frontend
WORKDIR /app/frontend
COPY firmeza.client/package*.json ./
RUN npm install
COPY firmeza.client/ ./
RUN npm run build

# Etapa 2: Compilación y Publicación de Backend .NET
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build-backend
WORKDIR /src

# Copiar archivos de proyectos para restaurar dependencias
COPY src/Firmeza.Domain/*.csproj src/Firmeza.Domain/
COPY src/Firmeza.Application/*.csproj src/Firmeza.Application/
COPY src/Firmeza.Infrastructure/*.csproj src/Firmeza.Infrastructure/
COPY src/Firmeza.Api/*.csproj src/Firmeza.Api/
COPY src/Firmeza.Web/*.csproj src/Firmeza.Web/

RUN dotnet restore src/Firmeza.Web/Firmeza.Web.csproj

# Copiar todo el código fuente
COPY src/ src/

# Copiar los artefactos del frontend al wwwroot del backend
COPY --from=build-frontend /app/frontend/dist/firmeza.client/browser/ src/Firmeza.Web/wwwroot/spa/

# Publicar la aplicación
RUN dotnet publish src/Firmeza.Web/Firmeza.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# Etapa 3: Runtime de Producción
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

# Instalar dependencias para renderizado de fuentes y documentos PDF en Linux (QuestPDF / SkiaSharp)
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

# Crear directorio de recibos persistente
RUN mkdir -p /app/wwwroot/recibos

COPY --from=build-backend /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Firmeza.Web.dll"]

