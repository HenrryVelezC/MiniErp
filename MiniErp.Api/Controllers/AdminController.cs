
// MiniErp.Api/Controllers/AdminController.cs

using Microsoft.AspNetCore.Authorization;                       // Atributos de autorización ([Authorize])
using Microsoft.AspNetCore.Identity;                            // Identity (UserManager, RoleManager, IdentityRole)
using Microsoft.AspNetCore.Mvc;                                 // ControllerBase, IActionResult, [ApiController]
using MiniErp.Infrastructure.Persistence;                       // AppUser (nuestro usuario de Identity)
using System;                                                   // Tipos básicos (Guid)
using System.ComponentModel.DataAnnotations;                    // DataAnnotations (Required)
using System.Linq;                                              // Linq (Select)
using System.Threading.Tasks;                                   // Task

namespace MiniErp.Api.Controllers
{
    /// <summary>
    /// Endpoints de administración: listar usuarios y asignar roles.
    /// Restringido por Policy "RequireAdmin".
    /// </summary>
    [ApiController]                                              // Manejo de validación automática para DTOs (400 BadRequest)
    [Route("api/[controller]")]                                  // Resuelve a: api/admin
    [Authorize(Policy = "RequireAdmin")]                         // Solo Admin puede acceder a este controller
    public class AdminController : ControllerBase
    {
        // Campos privados readonly para acceder a servicios de Identity.
        private readonly UserManager<AppUser> _userMgr;          // Crear/buscar usuarios, obtener roles, etc.
        private readonly RoleManager<IdentityRole<Guid>> _roleMgr; // Crear/consultar roles y existencia de roles

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// </summary>
        public AdminController(
            UserManager<AppUser> userMgr,
            RoleManager<IdentityRole<Guid>> roleMgr)
        {
            // IMPORTANTE: Asignamos los parámetros a los campos privados.
            // (ANTES: userMgr = userMgr; roleMgr = roleMgr; eso NO asignaba los campos)
            _userMgr = userMgr;
            _roleMgr = roleMgr;
        }

        /// <summary>
        /// Lista usuarios registrados mostrando datos básicos (Id, Email, DisplayName).
        /// </summary>
        /// <returns>200 OK con la lista proyectada</returns>
        [HttpGet("users")]
        public IActionResult Users()
        {
            // No materializamos la query con ToList() si no hace falta;
            // dejarlo como IQueryable es suficiente para Swagger/JSON, pero
            // si prefieres, puedes forzar ToList() para ejecutar la consulta aquí.
            var result = _userMgr.Users
                                 .Select(u => new { u.Id, u.Email, u.DisplayName })
                                 .ToList(); // << ejecutamos aquí para evitar evaluación diferida en serialización

            return Ok(result);
        }

        /// <summary>
        /// Asigna un rol a un usuario específico.
        /// Requiere que el rol exista previamente (SeedData crea: Admin, Manager, User).
        /// </summary>
        /// <param name="dto">UserId y Role</param>
        /// <returns>200 OK si asignó el rol; 400/404 en caso de error</returns>
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            // 1) Validación de existencia del rol (por ejemplo: "Admin", "Manager", "User")
            if (!await _roleMgr.RoleExistsAsync(dto.Role))
                return BadRequest($"El rol '{dto.Role}' no existe.");

            // 2) Buscar usuario por Id
            var user = await _userMgr.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
                return NotFound("Usuario no encontrado.");

            // 3) Asignar el rol al usuario
            var res = await _userMgr.AddToRoleAsync(user, dto.Role);
            if (!res.Succeeded)
                return BadRequest(res.Errors); // Devuelve lista de errores de Identity

            // 4) Respuesta exitosa
            return Ok(new { message = "Rol asignado." });
        }

        /// <summary>
        /// DTO para asignación de roles.
        /// Agregamos DataAnnotations mínimas para una mejor validación y mensajes 400 automáticos.
        /// </summary>
        public record AssignRoleDto(
            [Required] Guid UserId,                  // Requerido: el Id del usuario al que asignaremos el rol
            [Required] string Role                   // Requerido: nombre del rol (Admin/Manager/User, etc.)
        );
    }
}
