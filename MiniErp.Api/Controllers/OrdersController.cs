using Microsoft.AspNetCore.Authorization;                 // [Authorize] para exigir JWT y roles
using Microsoft.AspNetCore.Mvc;                           // Tipos de MVC: ControllerBase, ActionResult, atributos HTTP
using MiniErp.Application.Contracts;                      // Contratos de servicios (IOrderService)
using MiniErp.Application.DTOs;                           // DTOs: OrderReadDto, OrderUpsertDto, etc.
using System;                                             // Tipos básicos como Guid
using System.Collections.Generic;                         // List<T>
using System.Linq;                                        // LINQ: Select, Any, ToList
using System.Threading.Tasks;                             // Task y métodos async

namespace MiniErp.Api.Controllers
{
    /// <summary>
    /// CRUD de Pedidos (maestro) con sus Ítems (detalle).
    /// Requiere autenticación (JWT). Autorización por roles:
    /// - GET    : cualquier autenticado
    /// - POST/PUT: Admin o Manager
    /// - DELETE : solo Admin
    /// </summary>
    [ApiController]                                        // Valida modelos automáticamente y produce ProblemDetails por defecto
    [Route("api/[controller]")]                            // Ruta base: /api/orders
    [Authorize]                                            // Requiere estar autenticado (JWT válido)
    public class OrdersController : ControllerBase
    {
        // Campos privados readonly para dependencias (nomenclatura _camelCase)
        private readonly IOrderService _service;
        private readonly ILogger<OrdersController> _logger;


        /// <summary>
        /// Constructor con dependencias provistas por DI.
        /// </summary>
        public OrdersController(IOrderService service, ILogger<OrdersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>Obtiene todas las órdenes con su detalle.</summary>
        [HttpGet]                                          // GET /api/orders
        [ProducesResponseType(typeof(List<OrderReadDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<OrderReadDto>>> Get()
        {

            var result = await _service.GetAllAsync();
            return Ok(result);
                             // 200 OK con DTOs
        }

        /// <summary>Obtiene una orden por su Id (Guid).</summary>
        [HttpGet("{id:guid}")]                             // GET /api/orders/{id}
        [ProducesResponseType(typeof(OrderReadDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<OrderReadDto>> GetById(Guid id)
        {
            var dto = await _service.GetAsync(id);              // Busca el agregado
            if (dto is null) return NotFound();              // 404 si no existe
            return Ok(dto);                                 // 200 OK
        }

        /// <summary>Crea una nueva orden con su detalle.</summary>
        [HttpPost]                                         // POST /api/orders
        [Authorize(Roles = "Admin,Manager")]               // Solo Admin/Manager pueden crear
        [ProducesResponseType(typeof(OrderReadDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<OrderReadDto>> Create([FromBody] OrderUpsertDto dto)
        {
            // Validaciones mínimas de negocio (FluentValidation vendrá en el siguiente punto)
            if (dto is null) return BadRequest("Body requerido.");

            // Persistir a través del repositorio
            var created = await _service.CreateAsync(dto);

            // Log estructurado (Serilog) con propiedades nombradas
            _logger.LogInformation(
                "Order {OrderId} created for CustomerId {CustomerId}", created.Id, created.CustomerId);


            // 201 Created con Location apuntando a GET /api/orders/{id}
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Actualiza una orden existente (reemplazo total del detalle).</summary>
        [HttpPut("{id:guid}")]                             // PUT /api/orders/{id}
        [Authorize(Roles = "Admin,Manager")]               // Solo Admin/Manager pueden editar
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderUpsertDto dto)
        {
            // Validaciones mínimas de negocio
            if (dto is null) return BadRequest("Body requerido.");

            // Componer "nuevo estado" del agregado (el repositorio hará el reemplazo del detalle)
            var updated = await _service.UpdateAsync(id, dto);
            if (!updated) return NotFound();                     // 404 si no existe

            _logger.LogInformation("Order {OrderId} updated", id);

            return NoContent();                             // 204 sin body
        }

        /// <summary>Elimina una orden por Id (solo Admin).</summary>
        [HttpDelete("{id:guid}")]                          // DELETE /api/orders/{id}
        [Authorize(Roles = "Admin")]                       // Solo Admin
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);                   // Servicio debe manejar inexistente de forma idempotente
            _logger.LogWarning("Order {OrderId} deleted", id);
            return NoContent();                             // 204
        }
    }
}
