
```markdown
# 🚴 Bicicletería Backend

API REST para gestión de bicicletas y usuarios.  
Desarrollada con **ASP.NET Core 8.0**, **Entity Framework Core**, **PostgreSQL**, **JWT** y **Swagger**.

---

## 📋 Tabla de Contenidos

- [Características](#características)
- [Requisitos Previos](#requisitos-previos)
- [Instalación y Configuración](#instalación-y-configuración)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Base de Datos](#base-de-datos)
- [API Endpoints](#api-endpoints)
- [Autenticación JWT](#autenticación-jwt)
- [Swagger UI](#swagger-ui)
- [Datos Semilla](#datos-semilla)
- [Ejecución](#ejecución)
- [Solución de Problemas](#solución-de-problemas)
- [Paquetes NuGet](#paquetes-nuget)
- [Enlaces Útiles](#enlaces-útiles)

---

## ✨ Características

- ✅ Autenticación JWT (Bearer token, expiración configurable)
- ✅ Base de datos PostgreSQL con Entity Framework Core 8
- ✅ Migraciones automáticas y datos semilla
- ✅ Contraseñas hasheadas con BCrypt
- ✅ CORS habilitado (para comunicación con frontend)
- ✅ Documentación interactiva con **Swagger** (OpenAPI)
- ✅ Uso de variables de entorno (`.env`) para secretos
- ✅ Endpoints públicos y protegidos

---

## 🛠️ Requisitos Previos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) (12 o superior)
- Opcional: Visual Studio 2022 / VS Code / Git

---

## 📥 Instalación y Configuración

### 1. Clonar el repositorio

```bash
git clone https://github.com/rosalesAlan/Bicileteria-Backend.git
cd Bicileteria-Backend
```

### 2. Restaurar paquetes

```bash
dotnet restore
```

### 3. Configurar variables de entorno (archivo `.env`)

Crea un archivo `.env` en la raíz del proyecto con el siguiente contenido (basado en `.env.example`):

```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=BicicleteriaDB
DB_USER=postgres
DB_PASSWORD=tu_contraseña_aqui

JWT_KEY=clave_secreta_muy_larga_minimo_32_caracteres
JWT_ISSUER=https://bicicleteria.localhost
JWT_AUDIENCE=bicicleteria-app
JWT_EXPIRE_MINUTES=60
```

> ⚠️ **Nunca** subas el archivo `.env` al repositorio (está en `.gitignore`).  
> Puedes copiar `.env.example` como plantilla.

### 4. Crear la base de datos (si no existe)

Conéctate a PostgreSQL y ejecuta:

```sql
CREATE DATABASE "BicicleteriaDB";
```

### 5. Aplicar migraciones

```bash
dotnet ef database update
```

Esto creará las tablas `usuarios` y `productos`, e insertará los datos de prueba.

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
| price | NUMERIC(10,2) | Precio |
| imageurl | VARCHAR(500) | URL de la imagen |
| category | VARCHAR(100) | Categoría (Montaña, Ruta, Urbana, etc.) |

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
    "imageUrl": "https://...",
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
  "imageUrl": "https://...",
  "category": "BMX"
}
```
**Response (201):** el producto creado, con su `id` asignado.

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
