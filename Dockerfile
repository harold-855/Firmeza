# ==============================================================================
# 🏗️ FIRMEZA - Dockerfile Multi-Stage (Frontend Angular + Backend .NET 10 + Tests)
# ==============================================================================

# Etapa 1: Compilación de Frontend Angular SPA
FROM node:20-alpine AS build-frontend
WORKDIR /app/frontend
COPY firmeza.client/package*.json ./
RUN npm install
COPY firmeza.client/ ./
RUN npm run build

# Etapa 2: Compilación y Ejecución de Pruebas Unitarias Automatizadas
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS test-runner
WORKDIR /src
COPY Firmeza.slnx ./
COPY src/ src/
COPY tests/ tests/
RUN dotnet restore Firmeza.slnx
RUN dotnet test tests/Firmeza.UnitTests/Firmeza.UnitTests.csproj -c Release --logger "console;verbosity=normal"

# Etapa 3: Publicación de Backend .NET
FROM test-runner AS build-backend
WORKDIR /src
# Copiar los artefactos del frontend compilado al wwwroot de la aplicación web
COPY --from=build-frontend /app/frontend/dist/firmeza.client/browser/ src/Firmeza.Web/wwwroot/spa/
RUN dotnet publish src/Firmeza.Web/Firmeza.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# Etapa 4: Runtime de Producción
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
