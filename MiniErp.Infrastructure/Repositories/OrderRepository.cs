
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniErp.Application.Contracts;
using MiniErp.Domain.Entities;
using MiniErp.Infrastructure.Persistence;

namespace MiniErp.Infrastructure.Repositories
{
    /// <summary>Repositorio de Orders con EF Core (SQLite en dev).</summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;

        public OrderRepository(AppDbContext db) => _db = db;

        public async Task<Order?> GetAsync(Guid id) =>
            await _db.Orders
                     .Include(o => o.Items)
                     .FirstOrDefaultAsync(o => o.Id == id);

        public async Task<List<Order>> GetAllAsync() =>             // <- AQUÍ: sin '>' extra, con paréntesis
            await _db.Orders
                     .Include(o => o.Items)
                     .ToListAsync();

        public async Task<Order> CreateAsync(Order order)
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return order;
        }

        public async Task UpdateAsync(Order order)
        {
            _db.Orders.Update(order);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (existing != null)
            {
                _db.Orders.Remove(existing);
                await _db.SaveChangesAsync();
            }
        }
    }
}
