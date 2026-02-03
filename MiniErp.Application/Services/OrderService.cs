using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiniErp.Application.Contracts;
using MiniErp.Application.DTOs;
using MiniErp.Application.Mappings;

namespace MiniErp.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;

        public OrderService(IOrderRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<OrderReadDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(o => o.ToReadDto()).ToList();
        }

        public async Task<OrderReadDto?> GetAsync(Guid id)
        {
            var order = await _repo.GetAsync(id);
            return order?.ToReadDto();
        }

        public async Task<OrderReadDto> CreateAsync(OrderUpsertDto dto)
        {
            var entity = dto.ToEntity();
            entity.Validate();

            var created = await _repo.CreateAsync(entity);
            return created.ToReadDto();
        }

        public async Task<bool> UpdateAsync(Guid id, OrderUpsertDto dto)
        {
            var existing = await _repo.GetAsync(id);
            if (existing is null) return false;

            // Reemplazo total del estado (como tu controller hacía)
            existing.CustomerId = dto.CustomerId;
            existing.CustomerNameSnapshot = dto.CustomerNameSnapshot;

            existing.Items = dto.Items.Select(x => new Domain.Entities.OrderItem
            {
                ProductName = x.ProductName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice
            }).ToList();

            existing.Validate();

            await _repo.UpdateAsync(existing);
            return true;
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}