
using System;

namespace MiniErp.Domain.Entities
{
    /// <summary>
    /// Entidad detalle: Ítem de pedido.
    /// </summary>
    public class OrderItem
    {
        /// <summary>Identificador único del ítem.</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Referencia al pedido (FK lógica para mantener independencia del ORM).</summary>
        public Guid OrderId { get; set; }

        /// <summary>Nombre del producto.</summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>Cantidad solicitada (debe ser > 0).</summary>
        public int Quantity { get; set; }

        /// <summary>Precio unitario (>= 0).</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>Total de la línea (Quantity * UnitPrice).</summary>
        public decimal LineTotal => Quantity * UnitPrice;
    }
}
