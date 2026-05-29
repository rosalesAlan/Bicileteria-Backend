using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Bicicleteria.Backend.Data;
using DotNetEnv;

// ===== CARGAR VARIABLES DE ENTORNO DESDE .env =====
// DotNetEnv.Env.Load() debe ser llamado ANTES de construir la aplicación
// para que las variables de entorno estén disponibles cuando se cargue la configuración
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ===== AGREGAR PROVEEDOR DE VARIABLES DE ENTORNO =====
// Esto permite que ASP.NET Core use variables de entorno para reemplazar
// valores en la configuración, incluyendo placeholders como ${DB_HOST}
builder.Configuration.AddEnvironmentVariables();

// ===== HELPER: Expandir variables de entorno con sintaxis ${...} =====
static string ExpandEnvironmentVariables(string? input)
{
    if (string.IsNullOrEmpty(input)) return input ?? string.Empty;

    var pattern = @"\$\{([^}]+)\}";
    return System.Text.RegularExpressions.Regex.Replace(input, pattern, match =>
    {
        var envVar = match.Groups[1].Value;
        return System.Environment.GetEnvironmentVariable(envVar) ?? match.Value;
    });
}

// ===== 1. DBCONTEXT - Configurar AppDbContext con PostgreSQL =====
// Leer la cadena de conexión desde configuración y expandir variables de entorno
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    connectionString = ExpandEnvironmentVariables(connectionString);
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// ===== 2. AUTENTICACIÓN JWT =====
// Leer configuración JWT desde appsettings.json y expandir variables de entorno
var jwtKey = ExpandEnvironmentVariables(builder.Configuration["Jwt:Key"]) 
    ?? throw new InvalidOperationException("JWT Key no configurada en appsettings.json o en .env");
var jwtIssuer = ExpandEnvironmentVariables(builder.Configuration["Jwt:Issuer"]) 
    ?? throw new InvalidOperationException("JWT Issuer no configurado en appsettings.json o en .env");
var jwtAudience = ExpandEnvironmentVariables(builder.Configuration["Jwt:Audience"]) 
    ?? throw new InvalidOperationException("JWT Audience no configurado en appsettings.json o en .env");

// Para ExpireMinutes, expandir primero, luego parsear
var jwtExpireMinutesStr = ExpandEnvironmentVariables(builder.Configuration["Jwt:ExpireMinutes"]);
var jwtExpireMinutes = int.TryParse(jwtExpireMinutesStr, out var expireMinutes) 
    ? expireMinutes 
    : 60;

// Configurar esquema de autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validar emisor
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        
        // Validar audiencia
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        
        // Validar tiempo de expiración
        ValidateLifetime = true,
        
        // Validar clave de firma
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ===== 3. AUTORIZACIÓN =====
builder.Services.AddAuthorization();

// ===== 4. CORS - Política "AllowAll" para pruebas locales =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()        // Permitir cualquier origen
            .AllowAnyMethod()        // Permitir cualquier método (GET, POST, PUT, DELETE, etc.)
            .AllowAnyHeader();       // Permitir cualquier cabecera
    });
});

// ===== 5. CONTROLADORES =====
builder.Services.AddControllers();

var app = builder.Build();

// ===== PIPELINE DE MIDDLEWARE =====

// Usar CORS con la política "AllowAll"
app.UseCors("AllowAll");

// Usar autenticación
app.UseAuthentication();

// Usar autorización
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

app.Run();
