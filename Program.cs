using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Bicicleteria.Backend.Data;

var builder = WebApplication.CreateBuilder(args);

// ===== 1. DBCONTEXT - Configurar AppDbContext con PostgreSQL =====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ===== 2. AUTENTICACIÓN JWT =====
// Leer configuración JWT desde appsettings.json
var jwtKey = builder.Configuration["Jwt:Key"] 
    ?? throw new InvalidOperationException("JWT Key no configurada en appsettings.json");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
    ?? throw new InvalidOperationException("JWT Issuer no configurado en appsettings.json");
var jwtAudience = builder.Configuration["Jwt:Audience"] 
    ?? throw new InvalidOperationException("JWT Audience no configurado en appsettings.json");
var jwtExpireMinutes = int.TryParse(builder.Configuration["Jwt:ExpireMinutes"], out var expireMinutes) 
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
