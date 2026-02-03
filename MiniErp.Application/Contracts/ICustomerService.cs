using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiniErp.Application.DTOs;

namespace MiniErp.Application.Contracts
{
    public interface ICustomerService
    {
        Task<List<CustomerReadDto>> GetAllAsync();
        Task<CustomerReadDto?> GetAsync(Guid id);
        Task<CustomerReadDto> CreateAsync(CustomerUpsertDto dto);
        Task<bool> UpdateAsync(Guid id, CustomerUpsertDto dto);
        Task<bool> DeleteAsync(Guid Id);
    }
}
