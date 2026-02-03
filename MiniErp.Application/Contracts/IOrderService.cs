using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniErp.Application.DTOs;

namespace MiniErp.Application.Contracts
{
    public interface IOrderService
    {
        Task<List<OrderReadDto>> GetAllAsync();
        Task<OrderReadDto?> GetAsync(Guid id);
        Task<OrderReadDto> CreateAsync(OrderUpsertDto dto);
        Task<bool> UpdateAsync(Guid id, OrderUpsertDto dto);
        Task DeleteAsync(Guid id);
    }
}