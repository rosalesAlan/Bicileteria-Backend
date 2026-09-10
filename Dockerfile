# ===== STAGE 1: BUILD =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivo de proyecto
COPY ["Bicicleteria.Backend.csproj", "./"]

# Restaurar dependencias
RUN dotnet restore "Bicicleteria.Backend.csproj"

# Copiar código fuente completo
COPY . .

# Build de la aplicación en Release
RUN dotnet build "Bicicleteria.Backend.csproj" -c Release -o /app/build

# Publicar aplicación
RUN dotnet publish "Bicicleteria.Backend.csproj" -c Release -o /app/publish

# ===== STAGE 2: RUNTIME =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app

# Copiar artefactos publicados del stage anterior
COPY --from=build /app/publish .

# Exponer puerto (80 por defecto en Alpine)
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
	CMD wget --no-verbose --tries=1 --spider http://localhost/api/Health/connections || exit 1

# Ejecutar la aplicación
ENTRYPOINT ["dotnet", "Bicicleteria.Backend.dll"]
