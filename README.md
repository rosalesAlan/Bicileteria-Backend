
# 🚴 Bicicletería Backend

**API REST profesional** para gestión de bicicletas, categorías, carrusel y usuarios.  
Desarrollada con **ASP.NET Core 8.0**, **Entity Framework Core**, **PostgreSQL**, **MongoDB**, **JWT** y **Swagger**.

> **Tecnología Híbrida:** PostgreSQL para datos transaccionales + MongoDB para caché de productos.

---

## 📋 Tabla de Contenidos

- [Características](#características)
- [Arquitectura](#arquitectura)
- [Requisitos Previos](#requisitos-previos)
- [Instalación y Configuración](#instalación-y-configuración)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Base de Datos](#base-de-datos)
- [API Endpoints](#api-endpoints)
- [Autenticación y Autorización](#autenticación-y-autorización)
- [Caché con MongoDB](#caché-con-mongodb)
- [Swagger UI](#swagger-ui)
- [Prueba de Endpoints](#prueba-de-endpoints)
- [Datos Semilla](#datos-semilla)
- [Ejecución](#ejecución)
- [Solución de Problemas](#solución-de-problemas)
- [Paquetes NuGet](#paquetes-nuget)

---

## ✨ Características

- ✅ **Arquitectura de dos bases de datos**: PostgreSQL (OLTP) + MongoDB (Caché)
- ✅ **Autenticación JWT** con Bearer tokens y validación de issuer/audience
- ✅ **Control de roles**: endpoints protegidos solo para administradores
- ✅ **Caché inteligente**: productos servidos desde MongoDB con TTL configurable
- ✅ **CRUD completo** para: Productos, Categorías, Carrusel, Usuarios
- ✅ **Migraciones automáticas** con EF Core y datos semilla
- ✅ **Contraseñas** hasheadas con BCrypt
- ✅ **CORS habilitado** (AllowAll) para frontend y herramientas externas
- ✅ **Documentación interactiva** Swagger con botón Authorize para JWT
- ✅ **Variables de entorno** (.env) para configuración segura
- ✅ **Logging detallado** en startup y operaciones
- ✅ **Validación de integridad referencial** en eliminaciones

---

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────┐
│                   Frontend (React)                   │
└────────────────────────┬────────────────────────────┘
                         │
                    HTTPS/CORS
                         │
        ┌────────────────┴────────────────┐
        │                                 │
┌───────▼──────────────┐        ┌────────▼──────────┐
│   ASP.NET Core 8.0   │        │   Swagger UI       │
│   (REST API)         │        │   (Testing)        │
└───────┬──────────────┘        └────────┬───────────┘
        │                                │
        │ JWT Auth + CORS                │
        │                                │
    ┌───┴─────────────────────────────┬──┘
    │                                 │
┌───▼────────────────┐    ┌──────────▼────────┐
│   PostgreSQL       │    │   MongoDB         │
│ (Transaccional)    │    │ (Caché/Products)  │
│                    │    │                   │
│ • Users            │    │ • CachedProducts  │
│ • Products         │    │   (TTL)           │
│ • Categories       │    │ • Caché Status    │
│ • CarouselItems    │    │                   │
└────────────────────┘    └───────────────────┘
```

### Flujo de Datos

1. **GET /api/productos** (sin token):
   - Controlador consulta `MongoCacheService`
   - Si caché válido → retorna desde MongoDB
   - Si caché expirado → consulta PostgreSQL, actualiza MongoDB, retorna datos

2. **POST/PUT/DELETE** (solo admin):
   - Valida token JWT y rol "admin"
   - Modifica datos en PostgreSQL
   - Invalida caché en MongoDB
   - Próxima consulta recarga desde PostgreSQL

3. **Categorías y Carrusel** (públicos):
   - Consulta directamente PostgreSQL (datos pequeños)
   - Sin caché (bajo volumen)

---

## 🛠️ Requisitos Previos

### Obligatorio
- [**.NET 8.0 SDK**](https://dotnet.microsoft.com/download/dotnet/8.0) (o superior)
- [**PostgreSQL**](https://www.postgresql.org/download/) 12+ 
- [**MongoDB**](https://www.mongodb.com/try/download/community) 4.4+
- [**Git**](https://git-scm.com/)

### Opcional
- Visual Studio 2022 Community (IDE recomendado)
- VS Code + C# extension
- Postman o Insomnia (para pruebas API)

### Verificar instalación

```bash
# .NET
dotnet --version

# PostgreSQL
psql --version

# MongoDB (si está instalado)
mongosh --version
```

---

## 📥 Instalación y Configuración

### 1. Clonar el repositorio

```bash
git clone https://github.com/rosalesAlan/Bicileteria-Backend.git
cd Bicileteria-Backend
```

### 2. Restaurar paquetes NuGet

```bash
dotnet restore
```

### 3. Configurar variables de entorno (`.env`)

Crea archivo `.env` en la raíz del proyecto:

```env
# PostgreSQL
DB_HOST=localhost
DB_PORT=5432
DB_NAME=BicicleteriaDB
DB_USER=postgres
DB_PASSWORD=tu_contraseña_postgres

# MongoDB
MongoDb__ConnectionString=mongodb://localhost:27017

# JWT
JWT_KEY=tu_clave_secreta_super_larga_minimo_32_caracteres_aqui
JWT_ISSUER=https://bicicleteria.localhost
JWT_AUDIENCE=bicicleteria-app
JWT_EXPIRE_MINUTES=60

# Entorno
ASPNETCORE_ENVIRONMENT=Development
```

> ⚠️ **IMPORTANTE:** El archivo `.env` está en `.gitignore`. Nunca lo subas al repositorio.  
> Puedes copiar `.env.example` como plantilla.

### 4. Crear base de datos PostgreSQL

**Opción A: SQL directo**
```sql
CREATE DATABASE "BicicleteriaDB" ENCODING 'UTF8';
```

**Opción B: Con pgAdmin**
- Conectar a servidor local
- Clic derecho en "Databases" → Create → Database
- Nombre: `BicicleteriaDB`

**Opción C: Verificar conexión con psql**
```bash
psql -h localhost -U postgres -c "CREATE DATABASE \"BicicleteriaDB\";"
```

### 5. Verificar MongoDB

**Opción A: Local (servicio corriendo)**
```bash
# En Windows
net start MongoDB

# En Linux/Mac
brew services start mongodb-community
```

**Opción B: Cloud (MongoDB Atlas)**
- Crear cluster en [mongodb.com/cloud](https://www.mongodb.com/cloud)
- Copiar connection string en `.env` → `MongoDb__ConnectionString`

### 6. Aplicar Migraciones de EF Core

```bash
dotnet ef database update
```

**Output esperado:**
```
Building...
Applying migration '20240101000000_InitialCreate'.
...
Done.
```

Esto crea:
- Tablas: `roles`, `users`, `categories`, `products`, `carousel_items`
- Índices automáticos en EF
- Inserta **datos semilla** (usuarios, categorías, productos)

---

## ⚙️ Configuración

El proyecto usa **DotNetEnv** para cargar las variables del archivo `.env` y expandir placeholders en `appsettings.json`.  
No necesitas editar `appsettings.json` directamente; solo modifica `.env`.

El archivo `appsettings.json` ya contiene placeholders como `${DB_HOST}`, `${JWT_KEY}`, etc., que serán reemplazados automáticamente en tiempo de ejecución.

---

## 📁 Estructura del Proyecto

```
Bicileteria-Backend/
├── Controllers/
│   ├── AuthController.cs          # POST /api/auth/login
│   └── ProductsController.cs      # GET /api/products, POST /api/products
├── Data/
│   └── AppDbContext.cs            # DbContext y configuración de modelos
├── DTOs/
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   └── UserDto.cs
├── Models/
│   ├── User.cs
│   └── Product.cs
├── Migrations/                    # Migraciones generadas por EF Core
├── .env                           # Variables de entorno (ignorado por Git)
├── .env.example                   # Plantilla de ejemplo
├── appsettings.json               # Configuración con placeholders
├── Program.cs                     # Configuración de servicios y middleware
└── README.md                      # Este archivo
```

---

## 🗄️ Base de Datos

### Tabla `usuarios`

| Columna | Tipo | Descripción |
|---------|------|-------------|
| id | INTEGER PK | Identificador único |
| nombre | VARCHAR(100) | Nombre |
| apellido | VARCHAR(100) | Apellido |
| numerotelefono | VARCHAR(20) | Teléfono |
| mail | VARCHAR(200) UNIQUE | Correo electrónico |
| tipo | VARCHAR(50) | `admin`, `vendedor` o `cliente` |
| passwordhash | VARCHAR(500) | Hash BCrypt de la contraseña |

### Tabla `productos`

| Columna | Tipo | Descripción |
|---------|------|-------------|
| id | INTEGER PK | Identificador único |
| name | VARCHAR(200) | Nombre del producto |
| description | VARCHAR(1000) | Descripción |
| price | NUMERIC(10,2) | Precio unitario |
| category | VARCHAR(100) | Categoría (Montaña, Ruta, Urbana, etc.) |

**Nota:** Las imágenes se manejan por separado (no incluidas en la BD). Se acceden mediante un endpoint específico de files.

---

## 🔌 API Endpoints

### 🔓 Público (sin autenticación)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST   | `/api/auth/login` | Autenticar usuario y obtener token JWT |

**Request:**
```json
{
  "mail": "admin@bicileteria.com",
  "password": "Demo1234"
}
```

**Response (200):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": 1,
    "nombre": "Admin",
    "apellido": "Sistema",
    "mail": "admin@bicileteria.com",
    "tipo": "admin"
  }
}
```

---

### 🔒 Protegidos (requieren token JWT)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET    | `/api/products` | Obtener todos los productos |
| POST   | `/api/products` | Crear un nuevo producto |

**GET /api/products – Response (200):**
```json
[
  {
    "id": 1,
    "name": "Bicicleta Mountain Bike",
    "description": "Bicicleta de montaña de 26 pulgadas...",
    "price": 599.99,
    "category": "Montaña"
  }
]
```

**POST /api/products – Request:**
```json
{
  "name": "Bicicleta BMX",
  "description": "Para trucos y acrobacias",
  "price": 299.99,
  "category": "BMX"
}
```
**Response (201):** el producto creado, con su `id` asignado.

> **Nota:** Las imágenes se manejan por separado, no incluidas en la respuesta JSON. Acceso mediante endpoint específico de files.

---

## 🔐 Autenticación JWT

El sistema utiliza **Bearer tokens** (JWT) firmados con HMAC SHA256.

- **Header requerido:** `Authorization: Bearer <token>`
- **Expiración por defecto:** 60 minutos (configurable en `.env` con `JWT_EXPIRE_MINUTES`)

### Flujo de autenticación

1. El cliente envía `mail` y `password` a `/api/auth/login`.
2. El servidor valida credenciales (BCrypt) y genera un token JWT.
3. El cliente debe incluir el token en el header `Authorization` en cada request a endpoints protegidos.
4. El servidor valida el token (firma, expiración, issuer, audience) antes de procesar la request.

---

## 🧪 Swagger UI

La documentación interactiva de la API está disponible en **modo desarrollo** en:

```
http://localhost:5000/
```

### Cómo usar Swagger con autenticación

1. Ejecuta el proyecto (`dotnet run`).
2. Abre `http://localhost:5000`.
3. Ve al endpoint `POST /api/auth/login`, haz clic en **Try it out**, ingresa credenciales (ej. `admin@bicileteria.com` / `Demo1234`) y ejecuta.
4. Copia el valor de `accessToken` de la respuesta.
5. Haz clic en el botón **Authorize** (candado), escribe `Bearer <token_copiado>` y haz clic en **Authorize**.
6. Ahora puedes probar `GET /api/products` y `POST /api/products` sin errores 401.

> Swagger solo está habilitado cuando `ASPNETCORE_ENVIRONMENT=Development`. En producción no se expone.

---

## 👥 Datos Semilla

### Usuarios de prueba

| Email | Contraseña | Tipo |
|-------|------------|------|
| admin@bicileteria.com | Demo1234 | admin |
| usuario@bicileteria.com | Demo1234 | vendedor |
| cliente@bicileteria.com | Demo1234 | cliente |

### Productos de prueba

| Nombre | Precio | Categoría |
|--------|--------|-----------|
| Bicicleta Mountain Bike | $599.99 | Montaña |
| Bicicleta Ruta | $799.99 | Ruta |
| Bicicleta Urbana | $449.99 | Urbana |

Los datos se insertan automáticamente la primera vez que se aplican las migraciones.

---

## ▶️ Ejecución

### Modo desarrollo

```bash
dotnet run
```

El servidor escuchará en `http://localhost:5000` (y `https://localhost:5001` si está configurado).

### Verificar funcionamiento

- **Base de datos:** `dotnet ef database update --verbose`
- **Login:** (con `curl` o Swagger)  
  ```bash
  curl -X POST http://localhost:5000/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"mail":"admin@bicileteria.com","password":"Demo1234"}'
  ```
- **Productos (con token):**  
  Reemplaza `<TOKEN>` por el obtenido:
  ```bash
  curl -X GET http://localhost:5000/api/products \
    -H "Authorization: Bearer <TOKEN>"
  ```

---

## 🐛 Solución de Problemas

| Error | Posible solución |
|-------|------------------|
| `Connection refused` o `database does not exist` | Verifica que PostgreSQL esté corriendo y que la base de datos `BicicleteriaDB` exista. |
| `Authentication failed for user postgres` | Comprueba la contraseña en tu archivo `.env` (variable `DB_PASSWORD`). |
| `401 Unauthorized` en endpoints protegidos | El token puede haber expirado o no se incluyó correctamente. Vuelve a hacer login. |
| Swagger no carga (404) | Asegúrate de estar en entorno Development y que la URL sea `http://localhost:5000` (sin `/swagger` extra). |
| `app.Run()` duplicado (error histórico) | Ya está corregido: solo una llamada en `Program.cs`. |
| El endpoint login también pide token | Debe tener `[AllowAnonymous]`. Verifica que `AuthController` lo incluya. |

---

## 📦 Paquetes NuGet Utilizados

| Paquete | Versión | Uso |
|---------|---------|-----|
| Microsoft.EntityFrameworkCore | 8.0 | ORM base |
| Npgsql.EntityFrameworkCore.PostgreSQL | 8.0 | Proveedor para PostgreSQL |
| Microsoft.EntityFrameworkCore.Tools | 8.0 | Migraciones |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0 | Autenticación JWT |
| BCrypt.Net-Next | 4.0 | Hashing de contraseñas |
| DotNetEnv | 3.2 | Carga de `.env` |
| Swashbuckle.AspNetCore | 10.1.7 | Swagger / OpenAPI |

---

## 🔗 Enlaces Útiles

- [Documentación ASP.NET Core](https://learn.microsoft.com/es-es/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/es-es/ef/core/)
- [PostgreSQL](https://www.postgresql.org/docs/)
- [JWT.io](https://jwt.io/)
- [Swagger UI](https://swagger.io/tools/swagger-ui/)

---
