# ==============================================================================
# 🧪 FIRMEZA - Dockerfile Dedicado para Firmeza.Tests (tests/Dockerfile)
# Contexto de construcción recomendado: Raíz de la solución (.)
# ==============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS test-runner
WORKDIR /src

# 1. Copiar solución y proyectos para restauración
COPY Firmeza.slnx ./
COPY src/Firmeza.Domain/*.csproj src/Firmeza.Domain/
COPY src/Firmeza.Application/*.csproj src/Firmeza.Application/
COPY src/Firmeza.Infrastructure/*.csproj src/Firmeza.Infrastructure/
COPY src/Firmeza.Api/*.csproj src/Firmeza.Api/
COPY src/Firmeza.Web/*.csproj src/Firmeza.Web/
COPY tests/Firmeza.UnitTests/*.csproj tests/Firmeza.UnitTests/

# 2. Restaurar dependencias
RUN dotnet restore Firmeza.slnx

# 3. Copiar código y pruebas
COPY src/ src/
COPY tests/ tests/

# 4. Compilar solución completa
RUN dotnet build Firmeza.slnx -c Release --no-restore

# 5. Ejecutar pruebas unitarias
RUN dotnet test tests/Firmeza.UnitTests/Firmeza.UnitTests.csproj -c Release --no-build --logger "console;verbosity=normal"

ENTRYPOINT ["dotnet", "test", "tests/Firmeza.UnitTests/Firmeza.UnitTests.csproj", "-c", "Release", "--no-build", "--logger", "console;verbosity=normal"]
