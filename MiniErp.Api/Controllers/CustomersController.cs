using Microsoft.AspNetCore.Authorization;                 // [Authorize] para exigir JWT y roles
using Microsoft.AspNetCore.Mvc;                           // Tipos de MVC: ControllerBase, ActionResult, atributos HTTP
using MiniErp.Application.Contracts;                      // Contratos de servicios (ICustomerService)
using MiniErp.Application.DTOs;                           // DTOs de Customer
using System;                                             // Tipos básicos como Guid
using System.Collections.Generic;                         // List<T>
using System.Threading.Tasks;                             // Task y métodos async

namespace MiniErp.Api.Controllers
{
    [ApiController]                       // Habilita validación automática y comportamiento REST
    [Route("api/[controller]")]           // Ruta base: /api/customers
    [Authorize]                            // Requiere autenticación para todo el controller
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _service; // Lógica de negocio de Customer
        private readonly ILogger<CustomersController> _logger; // Logger del controller

        // Constructor con dependencias inyectadas por DI
        public CustomersController(
            ICustomerService service,
            ILogger<CustomersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet] // GET: /api/customers
        [ProducesResponseType(typeof(List<CustomerReadDto>), 200)] // Retorna lista de clientes
        [ProducesResponseType(401)]                                 // No autenticado
        public async Task<ActionResult<List<CustomerReadDto>>> Get()
        {
            var result = await _service.GetAllAsync(); // Obtiene todos los clientes
            return Ok(result);                          // HTTP 200
        }

        [HttpGet("{id:guid}")] // GET: /api/customers/{id}
        [ProducesResponseType(typeof(CustomerReadDto), 200)] // Cliente encontrado
        [ProducesResponseType(404)]                           // Cliente no existe
        [ProducesResponseType(401)]                           // No autenticado
        public async Task<ActionResult<CustomerReadDto>> GetById(Guid id)
        {
            var dto = await _service.GetAsync(id); // Busca cliente por Id
            if (dto is null) return NotFound();    // 404 si no existe
            return Ok(dto);                        // 200 si existe
        }

        [HttpPost] // POST: /api/customers
        [Authorize(Roles = "Admin,Manager")] // Solo Admin y Manager
        [ProducesResponseType(typeof(CustomerReadDto), 201)] // Cliente creado
        [ProducesResponseType(400)]                           // Body inválido
        [ProducesResponseType(401)]                           // No autenticado
        [ProducesResponseType(403)]                           // Sin permisos
        public async Task<ActionResult<CustomerReadDto>> Create(
            [FromBody] CustomerUpsertDto dto) // Datos del cliente
        {
            if (dto is null) return BadRequest("Body requerido."); // Validación básica

            var created = await _service.CreateAsync(dto); // Crea cliente

            _logger.LogInformation(
                "Customer {CustomerId} created", created.Id); // Log de auditoría

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created); // HTTP 201 + Location
        }

        [HttpPut("{id:guid}")] // PUT: /api/customers/{id}
        [Authorize(Roles = "Admin,Manager")] // Solo Admin y Manager
        [ProducesResponseType(204)]           // Actualización exitosa
        [ProducesResponseType(400)]           // Body inválido
        [ProducesResponseType(404)]           // Cliente no existe
        [ProducesResponseType(401)]           // No autenticado
        [ProducesResponseType(403)]           // Sin permisos
        public async Task<IActionResult> Update(Guid id, [FromBody] CustomerUpsertDto dto)
        {
            if (dto is null) return BadRequest("Body requerido."); // Validación básica

            var updated = await _service.UpdateAsync(id, dto); // Actualiza cliente
            if (!updated) return NotFound();                   // 404 si no existe

            _logger.LogInformation("Customer {CustomerId} updated", id); // Log de auditoría

            return NoContent(); // HTTP 204
        }

        [HttpDelete("{id:guid}")] // DELETE: /api/customers/{id}
        [Authorize(Roles = "Admin")] // Solo Admin
        [ProducesResponseType(204)]   // Eliminación exitosa
        [ProducesResponseType(404)]   // Cliente no existe
        [ProducesResponseType(401)]   // No autenticado
        [ProducesResponseType(403)]   // Sin permisos
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id); // Elimina cliente
            if (!deleted) return NotFound();               // 404 si no existe

            _logger.LogInformation("Customer {CustomerId} deleted", id); // Log de auditoría

            return NoContent(); // HTTP 204
        }
    }
}