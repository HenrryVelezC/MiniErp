
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniErp.Domain.Entities;

namespace MiniErp.Application.Contracts
{
    /// <summary>Contrato de acceso a datos para Order.</summary>
    public interface IOrderRepository
    {
        Task<Order?> GetAsync(Guid id);
        Task<List<Order>> GetAllAsync();              // <- IMPORTANTE: paréntesis correctos
        Task<Order> CreateAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(Guid id);
    }
}
