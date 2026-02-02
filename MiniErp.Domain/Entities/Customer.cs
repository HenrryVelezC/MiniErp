using System;
using MiniErp.Domain.Exceptions;

namespace MiniErp.Domain.Entities
{
    /// <summary>
    /// Entidad maestro: Cliente.
    /// Mantiene invariantes del dominio y evita estados inválidos.
    /// </summary>
    public class Customer
    {
        /// <summary>Identificador único del cliente.</summary>
        public Guid Id { get; private set; }

        /// <summary>Nombre del cliente (requerido).</summary>
        public string Name { get; private set; } = string.Empty;

        /// <summary>Correo electrónico del cliente (opcional).</summary>
        public string Email { get; private set; } = string.Empty;

        /// <summary>Teléfono del cliente (opcional).</summary>
        public string Phone { get; private set; } = string.Empty;

        /// <summary>Dirección del cliente (opcional).</summary>
        public string Address { get; private set; } = string.Empty;

        /// <summary>Fecha de creación del cliente (UTC).</summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Constructor requerido por algunos ORMs (EF Core).
        /// Protegido para evitar instanciación inválida desde fuera del dominio.
        /// </summary>
        protected Customer() { }

        /// <summary>
        /// Crea un cliente válido desde el dominio.
        /// </summary>
        public Customer(string name, string email = "", string phone = "", string address = "")
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;

            SetName(name);
            UpdateContactInfo(email, phone, address);

            // Opcional: asegura invariantes completas
            Validate();
        }

        /// <summary>
        /// Cambia el nombre asegurando la regla de negocio.
        /// </summary>
        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainValidationException("El nombre del cliente es requerido.");

            Name = name.Trim();
        }

        /// <summary>
        /// Actualiza información de contacto.
        /// Email/Phone/Address son opcionales, pero si email viene debe ser válido.
        /// </summary>
        public void UpdateContactInfo(string email = "", string phone = "", string address = "")
        {
            Email = (email ?? string.Empty).Trim();
            Phone = (phone ?? string.Empty).Trim();
            Address = (address ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email))
                throw new DomainValidationException("El correo electrónico del cliente no es válido.");
        }

        /// <summary>
        /// Validación completa del agregado/entidad (útil antes de persistir o en tests).
        /// </summary>
        public void Validate()
        {
            if (Id == Guid.Empty)
                throw new DomainValidationException("El Id del cliente es requerido.");

            if (string.IsNullOrWhiteSpace(Name))
                throw new DomainValidationException("El nombre del cliente es requerido.");

            if (!string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email))
                throw new DomainValidationException("El correo electrónico del cliente no es válido.");

            if (CreatedAt == default)
                throw new DomainValidationException("CreatedAt debe estar definido.");
        }

        private static bool IsValidEmail(string email)
        {
            // Validación simple (suficiente como regla inicial).
            // Si luego quieres más estricto, migra a ValueObject Email.
            return email.Contains("@") && email.Contains(".");
        }
    }
}