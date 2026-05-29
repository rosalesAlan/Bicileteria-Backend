# 🚀 Guía de Inicio Rápido - Biciletería Backend

Esta guía te ayudará a poner en marcha el backend en cuestión de minutos.

---

## ⚡ Inicio Rápido (5 minutos)

### 1. Verificar PostgreSQL

Abre pgAdmin o tu cliente de PostgreSQL favorito y verifica que:
- PostgreSQL esté corriendo
- Exista la base de datos `BicicleteriaDB`

### 2. Compilar el proyecto

```bash
dotnet build
```

**Esperado:** "Compilación correcta"

### 3. Aplicar migraciones

```bash
dotnet ef database update
```

**Esperado:**
- Tablas creadas: `usuarios`, `productos`
- 3 usuarios insertados
- 3 productos insertados

### 4. Ejecutar el servidor

```bash
dotnet run
```

**Esperado:** Servidor disponible en `http://localhost:5000` o similar

---

## 🧪 Pruebas Básicas

### Usando PowerShell

#### Test 1: Login con usuario admin

```powershell
$body = @{
	mail = "admin@bicileteria.com"
	password = "Demo1234"
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/login" `
	-Method POST `
	-ContentType "application/json" `
	-Body $body

$response.Content | ConvertFrom-Json | ConvertTo-Json
```

**Respuesta esperada:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
	"id": 1,
	"nombre": "Admin",
	"apellido": "Sistema",
	"mail": "admin@bicileteria.com",
	"tipo": "admin"
  }
}
```

#### Test 2: Login con credenciales inválidas

```powershell
$body = @{
	mail = "admin@bicileteria.com"
	password = "WrongPassword"
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/login" `
	-Method POST `
	-ContentType "application/json" `
	-Body $body -ErrorAction SilentlyContinue

$response.StatusCode  # Debería ser 401
```

### Usando cURL (o Postman)

```bash
# Test login exitoso
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"mail":"admin@bicileteria.com","password":"Demo1234"}'

# Test login fallido
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"mail":"admin@bicileteria.com","password":"WrongPassword"}'
```

---

## 📊 Verificar Base de Datos

### Conectarse a PostgreSQL

```bash
# Si tienes psql instalado
psql -h localhost -U postgres -d BicicleteriaDB
```

### Queries útiles

```sql
-- Ver usuarios
SELECT id, nombre, apellido, mail, tipo FROM usuarios;

-- Ver productos
SELECT id, nombre, precio, disponibilidad FROM productos;

-- Ver estructura de usuarios
\d usuarios

-- Ver estructura de productos
\d productos
```

---

## 🐛 Debugging

### Habilitar logs más detallados

Modifica `appsettings.json`:

```json
"Logging": {
  "LogLevel": {
	"Default": "Debug",
	"Microsoft.EntityFrameworkCore.Database.Command": "Debug"
  }
}
```

Ejecuta el proyecto con:

```bash
dotnet run
```

Verás todos los comandos SQL ejecutados en la consola.

### Ver logs de migración

```bash
dotnet ef database update --verbose
```

---

## 📝 Casos de Uso Comunes

### Crear un nuevo usuario

Para agregar un nuevo usuario a la base de datos manualmente:

```sql
INSERT INTO usuarios (nombre, apellido, numerotelefono, mail, tipo, passwordhash)
VALUES (
	'Juan',
	'Pérez',
	'+34 600 000 000',
	'juan@example.com',
	'cliente',
	'$2a$11$...'  -- Hash BCrypt de la contraseña
);
```

**Nota:** Los hashes BCrypt se generan en código. Usa BCrypt.Net para crear uno.

### Cambiar contraseña de usuario

```csharp
// En el código
var newHash = BCrypt.Net.BCrypt.HashPassword("NuevaContraseña");
user.PasswordHash = newHash;
_context.SaveChanges();
```

### Agregar un nuevo producto

```sql
INSERT INTO productos (nombre, descripcion, precio, disponibilidad)
VALUES (
	'Casco de Bicicleta',
	'Casco de seguridad con ventilación',
	49.99,
	50
);
```

---

## 🔑 Contraseñas de Prueba

Todos los usuarios tienen la misma contraseña para facilitar pruebas:

**Contraseña:** `Demo1234`

### Usuarios disponibles:

| Email | Contraseña | Tipo |
|-------|-----------|------|
| admin@bicileteria.com | Demo1234 | admin |
| usuario@bicileteria.com | Demo1234 | vendedor |
| cliente@bicileteria.com | Demo1234 | cliente |

---

## 📁 Estructura de archivos generados

Después de ejecutar las migraciones, verás:

```
Bicileteria-Backend/
├── bin/                           # Binarios compilados
├── obj/                           # Archivos de compilación
├── Migrations/
│   ├── 20260529004004_InitialCreate.cs
│   ├── 20260529004004_InitialCreate.Designer.cs
│   └── AppDbContextModelSnapshot.cs
└── ... (otros archivos del proyecto)
```

---

## ⚠️ Problemas Comunes

### "Compilación fallida"
- Verifica que tengas .NET 8.0 SDK instalado: `dotnet --version`
- Intenta: `dotnet clean` y luego `dotnet build`

### "La base de datos no existe"
```bash
# Crea la base de datos manualmente desde pgAdmin o:
psql -U postgres -c "CREATE DATABASE \"BicicleteriaDB\";"
```

### "Puerto 5000 ya está en uso"
```bash
# Cambia el puerto en Program.cs o corre con:
dotnet run --urls "http://localhost:5001"
```

### "Error de autenticación PostgreSQL"
- Verifica que el usuario `postgres` existe
- Verifica la contraseña en `appsettings.json`
- Reinicia PostgreSQL

---

## 📚 Siguientes Pasos

Después de verificar que todo funciona:

1. **Crear más endpoints** (GET, POST, PUT, DELETE para productos y usuarios)
2. **Implementar autorización** basada en roles (admin, vendedor, cliente)
3. **Agregar validaciones** en DTOs
4. **Crear tests unitarios**
5. **Documentar API con Swagger/OpenAPI**

---

## 🆘 Soporte

Si encuentras problemas:

1. Revisa el archivo `README.md` principal
2. Verifica los logs de la aplicación
3. Consulta la sección de Troubleshooting en README.md

---

**¡Listo para comenzar!** 🚀
