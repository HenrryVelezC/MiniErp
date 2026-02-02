
using System;
using System.Collections.Generic;
using System.Linq;
using MiniErp.Domain.Exceptions;

namespace MiniErp.Domain.Entities
{
    /// <summary>
    /// Entidad maestro: Pedido/Orden de venta.
    /// Reglas de negocio puras (sin dependencias de EF ni de infraestructura).
    /// </summary>
    public class Order
    {
        /// <summary>Identificador único del pedido.</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Cliente al que pertenece el pedido.</summary>
        public Guid CustomerId { get; set; }    
        public string CustomerNameSnapshot { get; set; } = string.Empty;

        /// <summary>Fecha de creación del pedido (UTC).</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Detalle: ítems del pedido.</summary>
        public List<OrderItem> Items { get; set; } = new();

        /// <summary>Total del pedido calculado a partir de los ítems.</summary>
        public decimal Total => Items.Sum(i => i.LineTotal);

        // --- Regla de negocio básica (opcional) ---
        // Si quieres validar que no se creen pedidos sin cliente ni ítems:
        public void Validate()
        {
            if (CustomerId == Guid.Empty)
                throw new DomainValidationException("CustomerId es requerido.");
                
            if (string.IsNullOrWhiteSpace(CustomerNameSnapshot))
                throw new DomainValidationException("CustomerNameSnapshot es requerido.");

            if (Items == null || Items.Count == 0)
                throw new DomainValidationException("El pedido debe tener al menos un ítem.");

            if (Items.Any(i => i.Quantity <= 0 || i.UnitPrice < 0))
                throw new DomainValidationException("Existen ítems con cantidad o precio inválidos.");
        }
    }
}
