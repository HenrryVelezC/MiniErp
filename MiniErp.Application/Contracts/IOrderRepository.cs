
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniErp.Domain.Entities;

namespace MiniErp.Application.Contracts
{

    /// <summary>
    /// Contrato (interfaz) para el acceso a datos de Orders.
    /// Parte esencial de Clean Architecture:
    /// - La capa Application define QUÉ se debe hacer,
    /// - La infraestructura define CÓMO se hace.
    /// </summary>
    public interface IOrderRepository
    {
        Task<Order?> GetAsync(Guid id);         // Obtiene un pedido por su Id (incluye items).
        Task<List<Order>> GetAllAsync();        // Obtiene todos los pedidos.
        Task<Order> CreateAsync(Order order);   // Crea un nuevo pedido con sus ítems.
        Task UpdateAsync(Order order);          // Actualiza un pedido existente y sus ítems.
        Task DeleteAsync(Guid id);              // Elimina un pedido por su Id (incluye ítems).
    }
}
