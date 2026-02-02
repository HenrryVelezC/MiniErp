using System;
using System.Collections.Generic;

namespace MiniErp.Application.DTOs
{

    /// <summary>
    /// DTO para lectura de pedidos, incluyendo el detalle (Items).
    /// Se envía hacia el cliente (API → Frontend).
    /// </summary>
    public record OrderReadDto(
        Guid Id,
        Guid CustomerId,
        string CustomerNameSnapshot,
        DateTime CreatedAt,
        List<OrderItemReadDto> Items
        );
    /// <summary>
    /// DTO para lectura de detalle de un pedido.
    /// </summary>
    public record OrderItemReadDto(
        Guid Id,
        string ProductName,
        int Quantity,
        decimal UnitPrice,
        decimal LineTotal
        );
    
    /// <summary>
    /// DTO para creación/edición de pedidos (datos recibidos desde el cliente).
    /// </summary>
    public record OrderUpsertDto(
        Guid CustomerId,
        string CustomerNameSnapshot,
        List<OrderItemUpsertDto> Items
        );
    /// <summary>
    /// DTO para creación/edición de detalle de un pedido.
    /// </summary>
    public record OrderItemUpsertDto(
        string ProductName,
        int Quantity,
        decimal UnitPrice
        );
}