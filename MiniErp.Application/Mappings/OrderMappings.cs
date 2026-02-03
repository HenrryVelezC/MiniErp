using System.Linq;
using MiniErp.Application.DTOs;
using MiniErp.Domain.Entities;

namespace MiniErp.Application.Mappings
{
    public static class OrderMappings
    {
        public static OrderReadDto ToReadDto(this Order o) =>
            new OrderReadDto(
                o.Id,
                o.CustomerId,
                o.CustomerNameSnapshot,
                o.CreatedAt,
                o.Items.Select(i => new OrderItemReadDto(
                    i.Id,
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    i.LineTotal
                )).ToList()
            );

        public static Order ToEntity(this OrderUpsertDto dto) =>
            new Order
            {
                CustomerId = dto.CustomerId,
                CustomerNameSnapshot = dto.CustomerNameSnapshot,
                Items = dto.Items.Select(x => new OrderItem
                {
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice
                }).ToList()
            };
    }
}