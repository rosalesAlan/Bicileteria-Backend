# ?? Quick Start - MongoCacheService

## Requisitos Previos

- ? .NET 8.0 SDK
- ? PostgreSQL en localhost:5432
- ? MongoDB en localhost:27017
- ? JWT_KEY configurada en .env

## 1. Iniciar MongoDB

### Opción A: Docker (Recomendado)
```bash
docker run -d --name mongodb -p 27017:27017 mongo:latest
```

### Opción B: Instalado localmente
```bash
mongod --dbpath C:\data\db
```

### Verificar conexión
```bash
mongo --version
# Debería mostrar: MongoDB shell version
```

## 2. Configurar appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=bicicleteria_db;...",
    "MongoDb": "mongodb://localhost:27017"
  },
  "MongoDb": {
    "DatabaseName": "BicicleteriaMDB",
    "CacheTtlMinutes": 15
  }
}
```

## 3. Compilar

```bash
cd Bicicleteria-Backend
dotnet build
```

**Resultado esperado:**
```
Compilación correcta.
0 Advertencia(s)
0 Errores
```

## 4. Ejecutar

```bash
dotnet run
```

**Resultado esperado:**
```
[INF] Now listening on: https://localhost:5001
[INF] ? Índice de MongoDB verificado/creado
```

## 5. Acceder a Swagger

```
https://localhost:5001
```

## 6. Obtener JWT Token

### Login
```bash
POST /api/auth/login
{
  "mail": "admin@bicileteria.com",
  "password": "Demo1234"
}
```

**Respuesta:**
```json
{
  "accessToken": "eyJhbGc...",
  "user": {
    "id": 1,
    "nombre": "Admin",
    "mail": "admin@bicileteria.com",
    "rol": "admin"
  }
}
```

Copia el `accessToken`

## 7. Probar el Caché

### 7.1 Ver estado del caché (vacío)
```bash
GET /api/products/cache/status
(Sin autenticación)
```

**Respuesta:**
```json
{
  "status": "sin_cache",
  "message": "No hay documentos de caché en MongoDB"
}
```

### 7.2 Obtener productos (primera vez - MISS)
```bash
GET /api/products
Authorization: Bearer {accessToken}
```

**Resultado:**
- ? Array de productos
- ?? Caché se carga en MongoDB
- ?? Tiempo de respuesta: 100-200ms

### 7.3 Ver estado del caché (activo)
```bash
GET /api/products/cache/status
```

**Respuesta:**
```json
{
  "status": "ok",
  "cacheId": "global",
  "productosCacheados": 5,
  "fechaActualizacion": "2026-01-06T18:30:00Z",
  "ttlMinutos": 15,
  "estaExpirado": false,
  "proximaExpiracion": "2026-01-06T18:45:00Z"
}
```

### 7.4 Obtener productos (segunda vez - HIT)
```bash
GET /api/products
Authorization: Bearer {accessToken}
```

**Resultado:**
- ? Array de productos (desde caché)
- ?? Tiempo de respuesta: 10-50ms (más rápido)

### 7.5 Crear producto (invalida caché)
```bash
POST /api/products
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "name": "Nueva Bicicleta",
  "description": "Descripción",
  "price": 799.99,
  "imageUrl": "https://...",
  "categoryId": 1,
  "availability": true
}
```

**Resultado:**
- ? Producto creado
- ?? Caché invalidada automáticamente

### 7.6 Invalidar caché manualmente
```bash
POST /api/products/cache/invalidate
```

**Respuesta:**
```json
{
  "message": "Caché invalidada correctamente"
}
```

## 8. Monitorear Logs

En la terminal donde corre `dotnet run`, verás logs como:

```
[INF] Intentando obtener productos desde caché de MongoDB
[INF] Caché expirada o no existe. Cargando desde SQL Server...
[INF] ? Caché actualizada. Guardados 5 productos en MongoDB
```

o en consultas posteriores:

```
[INF] Intentando obtener productos desde caché de MongoDB
[INF] ? Caché activa. Devolviendo 5 productos desde MongoDB
```

## 9. Troubleshooting

### ? "No se puede conectar a MongoDB"
```
Error: No connection could be made because the target machine actively refused it
```

**Solución:**
```bash
# Verificar que MongoDB está corriendo
docker ps | grep mongodb
# o
mongod --version
```

### ? "El token JWT es inválido"
```
Error 401 Unauthorized
```

**Solución:**
1. Login nuevamente con `/api/auth/login`
2. Copia el nuevo token
3. Usa en el header `Authorization: Bearer {token}`

### ? "La base de datos no existe"
```
Error: El nombre de la base de datos "BicicleteriaMDB" no existe
```

**Solución:** MongoDB crea la BD automáticamente en la primera operación. Solo asegúrate que MongoDB está corriendo.

## 10. Performance Metrics

### Tiempo de respuesta esperado

| Operación | Con Caché | Sin Caché | Mejora |
|-----------|-----------|----------|--------|
| GET /api/products (1era vez) | - | 100-200ms | - |
| GET /api/products (2da vez) | 10-50ms | 100-200ms | 80% ? |
| GET /api/products (dentro TTL) | 10-50ms | 100-200ms | 80% ? |
| GET /api/products (caché expirada) | - | 100-200ms | - |

### Hit Rate esperado

- Primera hora: ~95% HIT rate
- Después de crear: 0% (caché invalidada)
- Próxima consulta: ~95% HIT rate nuevamente

## 11. Arquitectura Visual

```
Client Request
      ?
GET /api/products
      ?
ProductsController
      ?
IMongoCacheService
      ??? MongoDB ? Si caché válida (HIT) ? Respuesta rápida
      ?
      ??? PostgreSQL ? Si caché expirada/no existe (MISS)
            ?
         Actualiza MongoDB
            ?
         Respuesta normal
```

## 12. Próximos Pasos

- [ ] Implementar warm-up de caché al iniciar
- [ ] Agregar métricas de Prometheus
- [ ] Implementar compresión de datos
- [ ] Soportar invalidación parcial
- [ ] Agregar caché por usuario

---

**¡Todo listo!** ??

Si todo funciona correctamente, deberías ver:
- ? Aplicación corriendo en https://localhost:5001
- ? Swagger accesible
- ? MongoDB almacenando caché
- ? Performance mejorando con cada consulta

**Documentación completa:** Ver `CACHE_DOCUMENTATION.md`  
**Resumen técnico:** Ver `IMPLEMENTATION_SUMMARY.md`
