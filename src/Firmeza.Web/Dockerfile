# ==============================================================================
# 🛡️ FIRMEZA.ADMIN - Dockerfile Producción (ASP.NET Core Razor Pages / MVC)
# Contexto de construcción: Raíz de la solución (.)
# ==============================================================================

# Etapa 1: Compilación de Frontend Angular SPA
FROM node:20-alpine AS build-frontend
WORKDIR /app/frontend
COPY firmeza.client/package*.json ./
RUN npm ci || npm install
COPY firmeza.client/ ./
RUN npx ng build --output-path=dist/browser --configuration production

# Etapa 2: Compilación y Publicación de Backend .NET
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build-backend
WORKDIR /src

COPY Firmeza.slnx ./
COPY src/Firmeza.Domain/*.csproj src/Firmeza.Domain/
COPY src/Firmeza.Application/*.csproj src/Firmeza.Application/
COPY src/Firmeza.Infrastructure/*.csproj src/Firmeza.Infrastructure/
COPY src/Firmeza.Web/*.csproj src/Firmeza.Web/

RUN dotnet restore src/Firmeza.Web/Firmeza.Web.csproj

COPY src/ src/

# Copiar los artefactos estáticos del frontend al directorio wwwroot/spa de la app web
COPY --from=build-frontend /app/frontend/dist/browser/ src/Firmeza.Web/wwwroot/spa/

RUN dotnet publish src/Firmeza.Web/Firmeza.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# Etapa 3: Runtime de Producción ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

# Instalar librerías nativas para QuestPDF y SkiaSharp en Linux
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

# Directorio persistente de recibos PDF
RUN mkdir -p /app/wwwroot/recibos

COPY --from=build-backend /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Firmeza.Web.dll"]
