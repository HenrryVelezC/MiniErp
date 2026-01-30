
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniErp.Application.Contracts;
using MiniErp.Domain.Entities;
using MiniErp.Infrastructure.Persistence;

namespace MiniErp.Infrastructure.Repositories
{
    
    /// <summary>
    /// Implementación del repositorio usando EF Core.
    /// Esta clase vive en Infrastructure porque depende de EF Core
    /// y del DbContext (dependencias externas).
    /// </summary>

    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;      //Esa línea declara una variable privada, de solo lectura, donde se guarda el DbContext que EF Core inyecta en el repositorio. Esa instancia se usa para consultar y modificar la base de datos.

        public OrderRepository(AppDbContext db){
             _db = db;                          // Constructor: recibe el DbContext a través de Inyección de Dependencias.
        }
        
        /// <summary>
        /// Obtiene un pedido por Id, incluyendo sus ítems (relación 1-n).
        /// AsNoTracking: optimiza lectura porque no lo vamos a modificar aquí.
        /// </summary>
        public async Task<Order?> GetAsync(Guid id) =>
            await _db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);


        /// <summary>
        /// Obtiene la lista completa de pedidos incluyendo ítems.
        /// AsNoTracking: mejora performance.
        /// </summary>
        public async Task<List<Order>> GetAllAsync() => 
            await _db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .ToListAsync();


        /// <summary>
        /// Crea un nuevo pedido y sus ítems.
        /// </summary>
        public async Task<Order> CreateAsync(Order order)
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            return order;
        }


        /// <summary>
        /// Actualiza un pedido existente.
        /// EF Core se encarga de trackear los cambios.
        /// </summary>
        public async Task UpdateAsync(Order order)
        {
            _db.Orders.Update(order);
            await _db.SaveChangesAsync();
        }


        /// <summary>
        /// Elimina un pedido si existe.
        /// </summary>
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
