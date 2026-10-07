# ==============================================================================
# 🌐 FIRMEZA.CLIENT - Dockerfile Producción (Angular 22 SPA + Nginx)
# ==============================================================================

# Etapa 1: Restaurar dependencias y compilar en modo Release / Production
FROM node:20-alpine AS build
WORKDIR /app

# Copiar paquetes e instalar dependencias
COPY package*.json ./
RUN npm ci || npm install

# Copiar código fuente y compilar bundle distribuible
COPY . .
RUN npx ng build --output-path=dist/browser --configuration production

# Etapa 2: Runtime Web Nginx Alpine Ligero
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copiar configuración optimizada de Nginx con soporte SPA PushState
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copiar los artefactos estáticos compilados
COPY --from=build /app/dist/browser .

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
