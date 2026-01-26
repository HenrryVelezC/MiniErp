
using System;

namespace MiniErp.Domain.Exceptions
{
    /// <summary>Excepción genérica de validación del dominio.</summary>
    public class DomainValidationException : Exception
    {
        public DomainValidationException(string message) : base(message) { }
    }
}
