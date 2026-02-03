using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniErp.Application.Contracts;
using MiniErp.Domain.Entities;
using MiniErp.Infrastructure.Persistence;

namespace MiniErp.Infrastructure.Repositories
{

    public class CustomerRepository : ICustomerRepository
    {

        // DbContext inyectado para acceso a la base de datos mediante EF Core
        public readonly AppDbContext _db; 

        public CustomerRepository(AppDbContext db){
             _db = db;                          // Constructor: recibe el DbContext a través de Inyección de Dependencias.
        }
        
        /// <summary>
        /// Obtiene un cliente por Id.
        /// AsNoTracking: optimiza lectura porque no lo vamos a modificar aquí.
        /// </summary>
        public async Task<Customer?> GetAsync(Guid id) =>
            await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        
        public async Task<IReadOnlyList<Customer>> GetAllAsync() => 
            await _db.Customers
                .AsNoTracking()
                .ToListAsync();
        
        public async Task<Customer> CreateAsync(Customer customer)
        {
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();
            return customer;
        }
    
        public async Task UpdateAsync(Customer customer)
        {
            _db.Customers.Update(customer);
            await _db.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid id)
        {
            var customer = await _db.Customers.FindAsync(id);
            if (customer != null)
            {
                _db.Customers.Remove(customer);
                await _db.SaveChangesAsync();
            }
        }
    }
}