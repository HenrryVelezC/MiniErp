using MiniErp.Application.DTOs;
using MiniErp.Domain.Entities;

namespace MiniErp.Application.Mappings
{
    public static class CustomerMappings
    {
        public static CustomerReadDto ToReadDto(this Customer c) =>
            new CustomerReadDto(c.Id, c.Name, c.Email, c.Phone, c.Address, c.CreatedAt);

        public static Customer ToEntity(this CustomerUpsertDto dto) =>
            new Customer(dto.Name, dto.Email, dto.Phone, dto.Address);
    }
}