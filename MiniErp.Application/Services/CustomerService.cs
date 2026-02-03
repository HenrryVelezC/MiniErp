using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MiniErp.Application.Contracts;
using MiniErp.Application.DTOs;
using MiniErp.Application.Mappings;

namespace MiniErp.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repo;

        public CustomerService(ICustomerRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<CustomerReadDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(c => c.ToReadDto()).ToList();
        }

        public async Task<CustomerReadDto?> GetAsync(Guid id)
        {
            var customer = await _repo.GetAsync(id);
            return customer?.ToReadDto();
        }

        public async Task<CustomerReadDto> CreateAsync(CustomerUpsertDto dto)
        {
            var entity = dto.ToEntity();
            entity.Validate();

            var created = await _repo.CreateAsync(entity);
            return created.ToReadDto();
        }

        public async Task<bool> UpdateAsync(Guid id, CustomerUpsertDto dto)
        {
            var existing = await _repo.GetAsync(id);
            if (existing is null) return false;

            // actualiza usando reglas del dominio
            existing.SetName(dto.Name);
            existing.UpdateContactInfo(dto.Email, dto.Phone, dto.Address);
            existing.Validate();

            await _repo.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _repo.GetAsync(id);
            if (existing is null) return false;

            await _repo.DeleteAsync(id);
            return true;
        }
    }
}