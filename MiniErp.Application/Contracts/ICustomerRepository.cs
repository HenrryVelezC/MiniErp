using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MiniErp.Domain.Entities;

namespace MiniErp.Application.Contracts
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetAsync(Guid id); //Obtiene el cliente por Id
        Task<IReadOnlyList<Customer>> GetAllAsync();     // Obtiene lista e todos los clientes
        Task<Customer> CreateAsync(Customer customer);      //Crea un nuevo Cliente
        Task UpdateAsync(Customer customer);    //Actualiza un cliente existente
        Task DeleteAsync(Guid id);              //Elimina un cliente por su id 
    }

}