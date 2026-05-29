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

// ===== 5. SWAGGER - Documentación API =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Bicicletería Backend API", Version = "v1" });
});

// ===== 6. CONTROLADORES =====
builder.Services.AddControllers();

// ===== 7. CONSTRUIR LA APLICACIÓN =====
var app = builder.Build();

// ===== PIPELINE DE MIDDLEWARE =====

// Archivos estáticos (necesario para Swagger UI)
app.UseStaticFiles();

// Usar Swagger (disponible en /swagger/index.html en desarrollo)
//if (app.Environment.IsDevelopment()) !!!!!!!!

// por ahora para testear Buscaremos que el sistema funcione en producción también, luego se puede restringir a desarrollo
if(true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bicicletería Backend API V1");
        c.RoutePrefix = string.Empty; // Hacer Swagger la página principal
        c.DefaultModelsExpandDepth(2);
        c.DefaultModelExpandDepth(2);
    });
}

// Usar CORS
app.UseCors("AllowAll");

// Usar autenticación
app.UseAuthentication();

// Usar autorización
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

app.Run();