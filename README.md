Aquí tienes la versión segura del `README.md`, sin credenciales expuestas y con instrucciones claras para que cada desarrollador configure sus propias claves:

---

```markdown
# 🚴 Bicicletería Backend

Backend API para gestión de bicicletas y usuarios. Desarrollado con **ASP.NET Core 8.0**, **Entity Framework Core**, **PostgreSQL** y **JWT Authentication**.

---

## 📋 Tabla de Contenidos

- [Características](#características)
- [Requisitos Previos](#requisitos-previos)
- [Instalación](#instalación)
- [Configuración](#configuración)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Base de Datos](#base-de-datos)
- [API Endpoints](#api-endpoints)
- [Autenticación](#autenticación)
- [Datos Semilla](#datos-semilla)
- [Ejecución del Proyecto](#ejecución-del-proyecto)
- [Notas Importantes](#notas-importantes)

---

## ✨ Características

- ✅ **Autenticación JWT** - Tokens seguros con expiración configurable
- ✅ **Base de Datos PostgreSQL** - Relacional y escalable
- ✅ **Entity Framework Core 8.0** - ORM moderno y eficiente
- ✅ **Migraciones Automáticas** - Control de versiones de base de datos
- ✅ **BCrypt Password Hashing** - Contraseñas seguras
- ✅ **CORS Habilitado** - Comunicación con frontends
- ✅ **Convenciones PostgreSQL** - Nombres de tablas y columnas en minúsculas
- ✅ **Datos Semilla** - Usuarios y productos de prueba pre-cargados

---

## 🛠️ Requisitos Previos

- **.NET 8.0 SDK** - [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
- **PostgreSQL 12+** - [Descargar](https://www.postgresql.org/download/)
- **Visual Studio 2022** o **VS Code**
- **Git** (opcional)

---

## 📥 Instalación

### 1. Clonar el repositorio

```bash
git clone https://github.com/rosalesAlan/Bicileteria-Backend.git
cd Bicileteria-Backend
```

### 2. Restaurar paquetes NuGet

```bash
dotnet restore
```

### 3. Configurar la base de datos

Asegúrate de que PostgreSQL esté corriendo. Si la base de datos **BicicleteriaDB** no existe, créala:

```sql
CREATE DATABASE "BicicleteriaDB";
```

### 4. Configurar el archivo de conexión

Creá un archivo `appsettings.json` en la raíz del proyecto basándote en el ejemplo de la sección [Configuración](#configuración). Reemplazá `TU_CONTRASEÑA` con la contraseña de tu usuario `postgres`.

### 5. Aplicar migraciones

```bash
dotnet ef database update
```

Esto creará las tablas `usuarios`, `productos` e insertará los datos semilla.

---

## ⚙️ Configuración

Creá un archivo `appsettings.json` en la raíz del proyecto con esta estructura:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=BicicleteriaDB;Username=postgres;Password=TU_CONTRASEÑA"
  },
  "Jwt": {
    "Key": "clave_secreta_super_larga_para_jwt_minimo_32_caracteres",
    "Issuer": "https://bicicleteria.localhost",
    "Audience": "bicicleteria-app",
    "ExpireMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

**Configuraciones importantes:**

| Clave | Descripción |
|-------|-------------|
| `ConnectionStrings.DefaultConnection` | Cadena de conexión a PostgreSQL. Reemplazá `TU_CONTRASEÑA` por tu clave real |
| `Jwt.Key` | Clave secreta para firmar tokens JWT (⚠️ mínimo 32 caracteres, cambiar en producción) |
| `Jwt.ExpireMinutes` | Tiempo de expiración de tokens en minutos |


---

## 📁 Estructura del Proyecto

```
Bicileteria-Backend/
├── Controllers/
│   └── AuthController.cs          # Endpoints de autenticación
├── Data/
│   └── AppDbContext.cs             # DbContext principal
├── DTOs/
│   ├── LoginRequest.cs             # DTO para login
│   ├── LoginResponse.cs            # DTO de respuesta
│   └── UserDto.cs                  # DTO de usuario
├── Models/
│   ├── User.cs                     # Modelo de usuario
│   └── Product.cs                  # Modelo de producto
├── Migrations/
│   └── InitialCreate.cs            # Migración inicial
├── appsettings.json                # Configuración (crear manualmente)
├── Program.cs                      # Punto de entrada
└── README.md                       # Este archivo
```

---

## 🗄️ Base de Datos

### Tabla: `usuarios`

| Columna | Tipo | Descripción |
|---------|------|-------------|
| id | INTEGER PK | Identificador único |
| nombre | VARCHAR(100) | Nombre del usuario |
| apellido | VARCHAR(100) | Apellido del usuario |
| numerotelefono | VARCHAR(20) | Teléfono de contacto |
| mail | VARCHAR(200) UNIQUE | Correo electrónico |
| tipo | VARCHAR(50) | Rol: 'admin', 'vendedor', 'cliente' |
| passwordhash | VARCHAR(500) | Contraseña hasheada con BCrypt |

### Tabla: `productos`

| Columna | Tipo | Descripción |
|---------|------|-------------|
| id | INTEGER PK | Identificador único |
| nombre | VARCHAR(200) | Nombre del producto |
| descripcion | VARCHAR(1000) | Descripción detallada |
| precio | NUMERIC(10,2) | Precio unitario |
| disponibilidad | INTEGER | Stock disponible |

---

## 🔌 API Endpoints

### Autenticación

#### **POST** `/api/auth/login`

Inicia sesión con email y contraseña.

**Request:**
```json
{
  "mail": "tumail@tmail.com",
  "password": "Contraseña1234"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "...",
  "user": {
    "id": 1,
    "nombre": "Nombre",
    "apellido": "Apellido",
    "mail": "tumail@mail.com",
    "tipo": "tipo"
  }
}
```

**Response (401 Unauthorized):**
```json
{
  "message": "Credenciales inválidas"
}
```

---

## 🔐 Autenticación

### Sistema de Tokens JWT

1. El cliente envía `mail` y `password` al endpoint `/api/auth/login`
2. El servidor valida las credenciales contra la base de datos
3. Si son válidas, genera un JWT con los datos del usuario
4. El cliente incluye el token en el header `Authorization: Bearer <token>` para futuras requests

---

## 👥 Datos Semilla

### Usuarios Predeterminados

| Nombre | Email | Tipo | Contraseña |
|--------|-------|------|-------------|
| Admin Sistema | admin@bicileteria.com | admin | Demo1234 |
| Usuario Vendedor | usuario@bicileteria.com | vendedor | Demo1234 |
| Cliente Ejemplo | cliente@bicileteria.com | cliente | Demo1234 |

> 🔐 Las contraseñas están hasheadas con BCrypt. No se pueden ver en texto plano en la base de datos.

### Productos Predeterminados

| Nombre | Descripción | Precio | Stock |
|--------|-------------|--------|-------|
| Bicicleta Mountain Bike | Bicicleta de montaña de 26" con suspensión delantera | $599.99 | 15 |
| Bicicleta Ruta | Bicicleta de ruta ligera y rápida para carreteras | $799.99 | 10 |
| Bicicleta Urbana | Bicicleta cómoda para desplazamientos en ciudad | $449.99 | 20 |

---

## ▶️ Ejecución del Proyecto

```bash
El servidor estará disponible en: `http://localhost:5000`

### Verificar que todo funciona

1. **Prueba de base de datos:**

```bash
dotnet ef database update --verbose
```

Deberías ver mensajes indicando que la migración se aplicó correctamente.

2. **Prueba de autenticación:**

Realiza una solicitud POST a `http://localhost:5000/api/auth/login` con:

```json
{
  "mail": "admin@bicileteria.com",
  "password": "Demo1234"
}
```

Si recibes un token JWT válido, la configuración es correcta.

3. **Prueba de productos:**

Usa el token obtenido y realiza una solicitud GET a `http://localhost:5000/api/products` con el header:
```

El servidor estará disponible en: `http://localhost:5000`

### Verificar que todo funciona

1. **Prueba de base de datos:**

```bash
dotnet ef database update --verbose
```

Deberías ver mensajes indicando que la migración se aplicó correctamente.

2. **Prueba de autenticación:**

Realiza una solicitud POST a `http://localhost:5000/api/auth/login` con:

```json
{
  "mail": "admin@bicileteria.com",
  "password": "Demo1234"
}
```

Si recibes un token JWT válido, la configuración es correcta.

3. **Prueba de productos:**

Usa el token obtenido y realiza una solicitud GET a `http://localhost:5000/api/products` con el header:

# El servidor estará disponible en: http://localhost:5000
```

---

## 🔍 Modelos de Datos

### User.cs

```csharp
public class User
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string NumeroTelefono { get; set; }
    public string Mail { get; set; }
    public string Tipo { get; set; }  // 'admin', 'vendedor', 'cliente'
    public string PasswordHash { get; set; }
}
```

### Product.cs

```csharp
public class Product
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Disponibilidad { get; set; }
}
```

---

## 📝 Notas Importantes


### Desarrollo
- Las migraciones deben ejecutarse antes de iniciar la aplicación
- Si modificás modelos, generá una nueva migración: `dotnet ef migrations add NombreMigracion`
- Los logs de Entity Framework están habilitados para debugging

### Producción
- Usar variables de entorno para credenciales en lugar de `appsettings.json`
- Configurar CORS más restrictivo
- Implementar HTTPS obligatorio

---

## 🐛 Troubleshooting

| Error | Solución |
|-------|----------|
| "Authentication failed for user postgres" | Verificá la contraseña en tu `appsettings.json` |
| "database does not exist" | Creá la base de datos: `CREATE DATABASE "BicicleteriaDB";` |
| "Connection refused" | Verificá que PostgreSQL esté corriendo |
| "password authentication failed" | Revisá usuario y contraseña en la cadena de conexión |

---

## 📚 Paquetes NuGet Utilizados

| Paquete | Uso |
|---------|-----|
| Microsoft.EntityFrameworkCore | ORM |
| Npgsql.EntityFrameworkCore.PostgreSQL | Driver PostgreSQL |
| Microsoft.EntityFrameworkCore.Tools | Migraciones |
| Microsoft.AspNetCore.Authentication.JwtBearer | Autenticación JWT |
| BCrypt.Net-Next | Hash de contraseñas |

---

## 🔗 Enlaces Útiles

- [Documentación ASP.NET Core](https://learn.microsoft.com/es-es/aspnet/core/)
- [Documentación Entity Framework Core](https://learn.microsoft.com/es-es/ef/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [JWT.io](https://jwt.io/)
```
