using Microsoft.AspNetCore.Authorization;                 // [Authorize] para exigir JWT y roles
using Microsoft.AspNetCore.Mvc;                           // Tipos de MVC: ControllerBase, ActionResult, atributos HTTP
using MiniErp.Application.Contracts;                      // Contrato del repositorio (IOrderRepository)
using MiniErp.Application.DTOs;                           // DTOs: OrderReadDto, OrderUpsertDto, etc.
using MiniErp.Domain.Entities;                            // Entidades de dominio: Order, OrderItem
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
        private readonly IOrderRepository _repo;
        private readonly ILogger<OrdersController> _logger;

        /// <summary>
        /// Constructor con dependencias provistas por DI.
        /// </summary>
        public OrdersController(IOrderRepository repo, ILogger<OrdersController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        /// <summary>Obtiene todas las órdenes con su detalle.</summary>
        [HttpGet]                                          // GET /api/orders
        [ProducesResponseType(typeof(List<OrderReadDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<OrderReadDto>>> Get()
        {
            // Obtiene la lista desde el repositorio (incluye Items)
            var list = await _repo.GetAllAsync();

            // Mapeo manual a DTOs (en siguientes puntos usaremos AutoMapper)
            var result = list.Select(o => new OrderReadDto(
                o.Id,
                o.CustomerName,
                o.CreatedAt,
                o.Items.Select(i => new OrderItemReadDto(
                    i.Id,
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    i.LineTotal
                )).ToList()
            )).ToList();

            return Ok(result);                              // 200 OK con DTOs
        }

        /// <summary>Obtiene una orden por su Id (Guid).</summary>
        [HttpGet("{id:guid}")]                             // GET /api/orders/{id}
        [ProducesResponseType(typeof(OrderReadDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<OrderReadDto>> GetById(Guid id)
        {
            var o = await _repo.GetAsync(id);              // Busca el agregado
            if (o is null) return NotFound();              // 404 si no existe

            // Mapeo manual a DTO de lectura
            var dto = new OrderReadDto(
                o.Id,
                o.CustomerName,
                o.CreatedAt,
                o.Items.Select(i => new OrderItemReadDto(
                    i.Id,
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    i.LineTotal
                )).ToList()
            );

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
            if (string.IsNullOrWhiteSpace(dto.CustomerName))
                return BadRequest("CustomerName es requerido.");
            if (dto.Items is null || dto.Items.Count == 0)
                return BadRequest("Debe incluir al menos un ítem.");
            if (dto.Items.Any(x => x.Quantity <= 0))
                return BadRequest("Quantity debe ser > 0.");
            if (dto.Items.Any(x => x.UnitPrice < 0))
                return BadRequest("UnitPrice debe ser >= 0.");

            // Componer el agregado de dominio (maestro + detalle)
            var order = new Order
            {
                CustomerName = dto.CustomerName,
                Items = dto.Items.Select(x => new OrderItem
                {
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice
                }).ToList()
            };

            // Persistir a través del repositorio
            var created = await _repo.CreateAsync(order);

            // Log estructurado (Serilog) con propiedades nombradas
            _logger.LogInformation("Order {OrderId} created for {Customer}", created.Id, created.CustomerName);

            // Mapear entidad creada a DTO de salida
            var result = new OrderReadDto(
                created.Id,
                created.CustomerName,
                created.CreatedAt,
                created.Items.Select(i => new OrderItemReadDto(
                    i.Id,
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    i.LineTotal
                )).ToList()
            );

            // 201 Created con Location apuntando a GET /api/orders/{id}
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, result);
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
            if (string.IsNullOrWhiteSpace(dto.CustomerName))
                return BadRequest("CustomerName es requerido.");
            if (dto.Items is null || dto.Items.Count == 0)
                return BadRequest("Debe incluir al menos un ítem.");
            if (dto.Items.Any(x => x.Quantity <= 0))
                return BadRequest("Quantity debe ser > 0.");
            if (dto.Items.Any(x => x.UnitPrice < 0))
                return BadRequest("UnitPrice debe ser >= 0.");

            // Comprobar existencia (mejor UX: devolver 404 si no existe)
            var exists = await _repo.GetAsync(id);
            if (exists is null) return NotFound();

            // Componer "nuevo estado" del agregado (el repositorio hará el reemplazo del detalle)
            var toUpdate = new Order
            {
                Id = id,
                CustomerName = dto.CustomerName,
                Items = dto.Items.Select(x => new OrderItem
                {
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice
                }).ToList()
            };

            // Delegamos la lógica de sincronización al repositorio (mejor separación de capas)
            await _repo.UpdateAsync(toUpdate);

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
            await _repo.DeleteAsync(id);                   // Repositorio debe manejar inexistente de forma idempotente
            _logger.LogWarning("Order {OrderId} deleted", id);
            return NoContent();                             // 204
        }
    }
}
