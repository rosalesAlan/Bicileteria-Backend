using System.Text;
using System.Text;
using Bicicleteria.Backend.Cache;
using Bicicleteria.Backend.Data;
using Bicicleteria.Backend.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

Console.WriteLine("[STARTUP] Iniciando aplicacion Bicicleteria Backend");


// Cargar variables de entorno desde .env
Env.Load();



var builder = WebApplication.CreateBuilder(args);

// Agregar variables de entorno a la configuración
builder.Configuration.AddEnvironmentVariables();



// ===== DBCONTEXT - PostgreSQL =====
Console.WriteLine("[STARTUP] ========== CONFIGURACION DE POSTGRESQL ==========");
var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "BicicleteriaDB";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

Console.WriteLine($"[STARTUP] Host PostgreSQL: {dbHost}");
Console.WriteLine($"[STARTUP] Puerto PostgreSQL: {dbPort}");
Console.WriteLine($"[STARTUP] Base de datos: {dbName}");
Console.WriteLine($"[STARTUP] Usuario: {dbUser}");
Console.WriteLine($"[STARTUP] Contrasena: {'*' * (dbPassword?.Length ?? 0)}");

var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

Console.WriteLine("[STARTUP] AppDbContext registrado para PostgreSQL");

// ===== MONGODB - Servicio de caché =====
Console.WriteLine("[STARTUP] ========== CONFIGURACION DE MONGODB ==========");
var mongoDbConnectionString = builder.Configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017";
Console.WriteLine($"[STARTUP] Connection string MongoDB: {mongoDbConnectionString}");

var mongoClient = new MongoClient(mongoDbConnectionString);
builder.Services.AddSingleton<IMongoClient>(mongoClient);

builder.Services.AddScoped<IMongoCacheService, MongoCacheService>();


// ===== SERVICIO DE VERIFICACION DE CONEXIONES =====

builder.Services.AddScoped<IConnectionVerificationService, ConnectionVerificationService>();


// ===== CONTROLADORES =====

builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
Console.WriteLine("[STARTUP] Endpoints API Explorer agregado");

// ===== SWAGGER =====
Console.WriteLine("[STARTUP] ========== CONFIGURANDO SWAGGER ==========");
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bicicleteria API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer scheme",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
Console.WriteLine("[STARTUP] Swagger configurado");

// ===== JWT AUTHENTICATION =====
Console.WriteLine("[STARTUP] ========== CONFIGURANDO JWT AUTHENTICATION ==========");
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
{
    Console.WriteLine("[STARTUP] ERROR: JWT_KEY no esta configurada o es muy corta");
    throw new InvalidOperationException("JWT_KEY debe tener al menos 32 caracteres y estar definida en .env");
}
var key = Encoding.UTF8.GetBytes(jwtKey);
Console.WriteLine("[STARTUP] JWT Key cargada (32+ caracteres)");

var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "https://bicicleteria.localhost";
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "bicicleteria-app";

Console.WriteLine($"[STARTUP] JWT Issuer: {issuer}");
Console.WriteLine($"[STARTUP] JWT Audience: {audience}");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
Console.WriteLine("[STARTUP] JWT Bearer configurado");

// ===== CORS =====
Console.WriteLine("[STARTUP] ========== CONFIGURANDO CORS ==========");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
Console.WriteLine("[STARTUP] CORS configurado con politica AllowAll");

Console.WriteLine("[STARTUP] Construyendo aplicacion...");
var app = builder.Build();

Console.WriteLine("[STARTUP] Aplicacion construida exitosamente");

// ===== VERIFICAR CONEXIONES AL INICIAR =====
Console.WriteLine("[STARTUP] ========== VERIFICACION DE CONEXIONES ==========");
using (var scope = app.Services.CreateScope())
{
    var connectionService = scope.ServiceProvider.GetRequiredService<IConnectionVerificationService>();
    Console.WriteLine("[STARTUP] Iniciando verificacion de conexiones a bases de datos");

    try
    {
        var allConnections = await connectionService.VerifyAllConnectionsAsync();

        if (allConnections.AllConnected)
        {
            Console.WriteLine("[STARTUP] RESULTADO: TODAS LAS CONEXIONES EXITOSAS");
        }
        else
        {
            Console.WriteLine("[STARTUP] RESULTADO: UNA O MAS CONEXIONES FALLARON");
            if (!allConnections.PostgresStatus.IsConnected)
            {
                Console.WriteLine($"[STARTUP] PostgreSQL FALLO: {allConnections.PostgresStatus.Message}");
            }
            if (!allConnections.MongoDbStatus.IsConnected)
            {
                Console.WriteLine($"[STARTUP] MongoDB FALLO: {allConnections.MongoDbStatus.Message}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[STARTUP] ERROR verificando conexiones: {ex.Message}");
    }
}


// ===== SWAGGER UI =====
Console.WriteLine("[STARTUP] ========== CONFIGURANDO SWAGGER UI ==========");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bicicleteria API v1");
    c.DocumentTitle = "Bicicleteria API Documentation";
    c.DefaultModelsExpandDepth(0);
});
Console.WriteLine("[STARTUP] Swagger UI configurado en /swagger");

Console.WriteLine($"[STARTUP] Entorno: {app.Environment.EnvironmentName}");

// ===== MIDDLEWARES EN ORDEN CORRECTO =====
Console.WriteLine("[STARTUP] ========== APLICANDO MIDDLEWARES ==========");
app.UseHttpsRedirection();
Console.WriteLine("[STARTUP] HTTPS Redirection habilitado");

app.UseCors("AllowAll");
Console.WriteLine("[STARTUP] CORS aplicado (AllowAll)");

app.UseAuthentication();
Console.WriteLine("[STARTUP] Autenticación habilitada");

app.UseAuthorization();
Console.WriteLine("[STARTUP] Autorización habilitada");

app.MapControllers();
Console.WriteLine("[STARTUP] Controladores mapeados");

Console.WriteLine("[STARTUP] ========== APLICACION LISTA ==========");
Console.WriteLine("[STARTUP] Swagger disponible en: https://localhost:7164/swagger");
Console.WriteLine("[STARTUP] Ejecutando aplicacion...");

var passwordHash1 = BCrypt.Net.BCrypt.HashPassword("Demo1234");
Console.WriteLine($"Hash de contraseña para Demo1234: {passwordHash1}");

app.Run();
