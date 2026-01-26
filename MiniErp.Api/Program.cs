
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using MiniErp.Infrastructure.Persistence;
using MiniErp.Infrastructure;
using MiniErp.Application.Contracts;
using MiniErp.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1) Serilog: logging estructurado (console + file)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// 2) EF Core + SQLite (ruta relativa a la API)
var cs = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(cs));

// 3) Identity con Guid
builder.Services
    .AddIdentity<AppUser, IdentityRole<Guid>>(options =>
    {
        // Opciones básicas de seguridad (ajusta según política)
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 4) Autenticación JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            ClockSkew = TimeSpan.Zero
        };
    });

// 5) Autorización por Roles/Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
});

// 6) DI de repositorios (Application -> Infrastructure)
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// 7) Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 8) Seed básico (roles y admin) al levantar la app
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

// 9) Middlewares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(); // trazabilidad por request/response
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
