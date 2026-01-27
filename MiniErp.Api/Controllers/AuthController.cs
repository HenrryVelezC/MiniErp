
// MiniErp.Api/Controllers/AuthController.cs

using Microsoft.AspNetCore.Authorization;              // Atributos de Autorización (Authorize/AllowAnonymous)
using Microsoft.AspNetCore.Identity;                   // Identity (UserManager/SignInManager)
using Microsoft.AspNetCore.Mvc;                        // ControllerBase + ActionResult
using Microsoft.IdentityModel.Tokens;                  // JWT: SigningCredentials, SecurityKey
using MiniErp.Infrastructure.Persistence;              // AppUser (Identity)
using System.IdentityModel.Tokens.Jwt;                 // JwtSecurityToken, JwtSecurityTokenHandler
using System.Security.Claims;                          // Claims
using System.Text;                                     // Encoding
using System.ComponentModel.DataAnnotations;           // [Required], [EmailAddress]
using System.Linq;                                     // Linq (Select)
using System.Threading.Tasks;                          // Task
using System;

namespace MiniErp.Api.Controllers
{
    /// <summary>
    /// Endpoints básicos para registro y login con JWT.
    /// </summary>
    [ApiController]                                      // Convierte errores de validación en 400 automáticos, binding, etc.
    [Route("api/[controller]")]                          // Resuelve a: api/auth
    public class AuthController : ControllerBase
    {
        // Campos readonly para servicios inyectados.
        private readonly UserManager<AppUser> _userMgr;   // Gestiona usuarios (crear, buscar, roles, etc.)
        private readonly SignInManager<AppUser> _signInMgr; // Gestiona login (verifica contraseña, lockout)
        private readonly IConfiguration _config;          // Lee configuración (Issuer, Audience, Key)

        /// <summary>
        /// El constructor recibe las dependencias por Inyección de Dependencias (DI).
        /// </summary>
        public AuthController(
            UserManager<AppUser> userMgr,
            SignInManager<AppUser> signInMgr,
            IConfiguration config)
        {
            // Asignamos a campos privados. (ANTES: userMgr = userMgr; -> eso NO asigna el campo)
            _userMgr = userMgr;
            _signInMgr = signInMgr;
            _config = config;
        }

        /// <summary>
        /// Crea un usuario nuevo. Restringido a la Policy "RequireAdmin".
        /// </summary>
        /// <remarks>
        /// Body de ejemplo:
        /// {
        ///   "email": "user1@minierp.local",
        ///   "password": "User123$",
        ///   "displayName": "User 1"
        /// }
        /// </remarks>
        [HttpPost("register")]
        [Authorize(Policy = "RequireAdmin")] // Solo Admin puede registrar usuarios.
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Si el modelo es inválido (por atributos [Required]/[EmailAddress]), ApiController retornará 400 automáticamente.
            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                DisplayName = dto.DisplayName ?? dto.Email
            };

            // Crea el usuario con el password indicado (Identity valida política configurada en Program.cs).
            var result = await _userMgr.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                // Devolvemos 400 con los errores de Identity (ej.: password no cumple policy).
                return BadRequest(result.Errors);
            }

            // Rol por defecto "User"; si necesitas otro, cámbialo aquí o en un endpoint de administración.
            await _userMgr.AddToRoleAsync(user, "User");

            return Ok(new { message = "Usuario creado" });
        }

        /// <summary>
        /// Autentica un usuario y retorna un JWT de acceso.
        /// </summary>
        /// <remarks>
        /// Body de ejemplo:
        /// {
        ///   "email": "admin@minierp.local",
        ///   "password": "Admin123$"
        /// }
        /// </remarks>
        [HttpPost("login")]
        [AllowAnonymous] // Endpoint público (no requiere token).
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // 1) Buscamos por email
            var user = await _userMgr.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(); // Evitamos decir "no existe el usuario" por seguridad.

            // 2) Verificamos contraseña. lockoutOnFailure:true activará el bloqueo si hay intentos fallidos consecutivos.
            var result = await _signInMgr.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
                return Unauthorized();

            // 3) Generamos un JWT que incluye roles como claims.
            var token = await GenerateJwtAsync(user);

            // 4) Retornamos el token al cliente.
            return Ok(new { token });
        }

        /// <summary>
        /// Retorna información del usuario autenticado (según el JWT enviado).
        /// </summary>
        [HttpGet("me")]
        [Authorize] // Requiere JWT válido.
        public async Task<IActionResult> Me()
        {
            // Tomamos el userId del claim del token.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var user = await _userMgr.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();

            // Obtenemos roles asociados al usuario.
            var roles = await _userMgr.GetRolesAsync(user);

            // Retornamos datos básicos (evitar exponer información sensible).
            return Ok(new { user.Email, user.DisplayName, roles });
        }

        /// <summary>
        /// Construye un JWT con claims básicos + roles del usuario.
        /// </summary>
        private async Task<string> GenerateJwtAsync(AppUser user)
        {
            // Leemos la configuración de JWT desde appsettings: Jwt:Issuer, Jwt:Audience, Jwt:Key.
            var jwtSection = _config.GetSection("Jwt");

            // Clave simétrica para firmar el token (HMAC-SHA256). En producción, manejar por Secret Manager o Key Vault.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));

            // Credenciales de firmado.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Obtenemos roles del usuario y los agregamos como claims (ClaimTypes.Role).
            var roles = await _userMgr.GetRolesAsync(user);

            // Claims básicos del usuario. Evita claims innecesarias (principio de mínimo privilegio).
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName!),
                new(ClaimTypes.Email, user.Email!)
            };

            // Añadimos roles
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            // Construimos el token con expiración corta (p. ej., 2 horas). El refresh token sería otro endpoint/sistema.
            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            // Serializamos el token a string (compact JWS).
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    /// <summary>
    /// Modelo para registrar usuario (entrada desde cliente).
    /// Se añaden validaciones con DataAnnotations para 400 automáticos vía [ApiController].
    /// </summary>
/*    public record RegisterDto(
        [property: Required, EmailAddress] string Email,
        [property: Required] string Password,
        string? DisplayName);
*/
  
// ✅ Anotaciones en los PARÁMETROS
    public record RegisterDto(
        [Required, EmailAddress] string Email,
        [Required, MinLength(6)] string Password,
        [Required] string DisplayName
    );
  /// <summary>
    /// Modelo para login.
    /// </summary>
    public record LoginDto(
        [Required, EmailAddress] string Email,
        [Required] string Password
    );

}