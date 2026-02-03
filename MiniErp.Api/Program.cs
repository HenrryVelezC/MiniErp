using System;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using MiniErp.Infrastructure.Persistence;
using MiniErp.Infrastructure;
using MiniErp.Application.Contracts;
using MiniErp.Application.Services;
using MiniErp.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Repositorios (Infrastructure)
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// 1) Registrar CORS (perfil de desarrollo, abierto)
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200") // origen del dev server de Angular
            .AllowAnyHeader()
            .AllowAnyMethod();                     // GET, POST, PUT, DELETE, OPTIONS
            //.AllowCredentials();                  // si más adelante usas cookies
    });
});


// -----------------------------
// 1) Serilog: logging estructurado
// Lee config desde appsettings, enriquece con contexto y registra Serilog como logger del host
// -----------------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// -----------------------------
// 2) EF Core + SQLite
// Registra el AppDbContext usando la cadena de conexión "Default"
// -----------------------------
var cs = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(cs));

// -----------------------------
// 3) Identity (usuarios y roles) usando Guid como key
// Ajusta algunas reglas básicas de contraseña y usa AppDbContext como store
// -----------------------------
builder.Services
    .AddIdentity<AppUser, IdentityRole<Guid>>(options =>
    {
        // Reglas mínimas (ajusta según políticas de seguridad)
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>() // Tablas de Identity dentro de tu DbContext
    .AddDefaultTokenProviders();              // Tokens para reset password, email confirm, etc.

// -----------------------------
// 4) Autenticación con JWT Bearer
// Lee sección Jwt (Issuer, Audience, Key) y define parámetros de validación del token
// -----------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero,

        // 👇 asegura que el rol se lea correctamente desde el token
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
    };


    // (Opcional) Logs útiles para depurar JWT
    // opt.Events = new JwtBearerEvents
    // {
    //     OnAuthenticationFailed = ctx =>
    //     {
    //         Log.Error(ctx.Exception, "JWT auth failed");
    //         return Task.CompletedTask;
    //     }
    // };
});

/// -----------------------------
/// Esto evita que Identity envíe:
/// ❌ /Account/Login
/// ❌ /Account/AccessDenied
/// Y en su lugar envía:
///✔ 401 (cuando no tiene token)
///✔ 403 (cuando no tiene permisos)
/// Configuración de cookies para autenticación
/// -----------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401; // Unauthorized
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403; // Forbidden
        return Task.CompletedTask;
    };
});

// -----------------------------
// 5) Autorización (políticas/roles)
// Crea la política "RequireAdmin" que exige el rol Admin
// -----------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
});

// -----------------------------
// 6) Inyección de dependencias de repositorios
// Application depende de IOrderRepository; Infrastructure provee OrderRepository
// Vida Scoped (una instancia por request)
// -----------------------------
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// -----------------------------
// 7) Controllers + Swagger/OpenAPI
// -----------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // 🔐 Definir esquema de seguridad JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresa el token JWT usando este formato: Bearer {tu_token}"
    });

    // 🔐 Requerir el esquema de seguridad para TODOS los endpoints
    // Esto hace que Swagger nunca intente usar cookies (que provocarían otra vez /Account/Login).
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var allowedOrigins = new[] { "http://localhost:4200" /* si usas https, añade "https://localhost:4200" */ };



var app = builder.Build();

// -----------------------------
// 8) Seed de datos al iniciar la app
// Crea roles/usuario admin si no existen (usa un scope para resolver servicios)
// -----------------------------
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

// -----------------------------
// 9) Middlewares del pipeline HTTP
// - Swagger solo en desarrollo
// - Logging por request con Serilog
// - Redirección a HTTPS
// - Autenticación y autorización
// - Mapeo de controllers
// -----------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(); // logs de cada request/response con tiempos
//app.UseHttpsRedirection();

app.UseCors("FrontendDev"); // aplica política de CORS

app.UseAuthentication(); // valida tokens/identidad del usuario
app.UseAuthorization();  // aplica roles/políticas

app.MapControllers();    // expone endpoints de tus controllers

app.Run();               // arranca la aplicación
