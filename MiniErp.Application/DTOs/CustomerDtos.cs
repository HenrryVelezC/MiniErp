using System;
using System.Collections.Generic;

namespace MiniErp.Application.DTOs
{
    /// <summary>
    /// DTO para lectura de clientes.
    /// Se envía hacia el cliente (API → Frontend).
    /// </summary>
    public record CustomerReadDto(
        Guid Id,
        string Name,
        string Email,
        string Phone,
        string Address,
        DateTime CreatedAt
        );

    /// <summary>
    /// DTO para creación/edición de clientes (datos recibidos desde el cliente).
    /// </summary>
    public record CustomerUpsertDto(
        string Name,
        string Email,
        string Phone,
        string Address
        );
}