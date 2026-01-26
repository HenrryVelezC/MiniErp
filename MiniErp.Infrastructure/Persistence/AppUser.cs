
using Microsoft.AspNetCore.Identity;
using System;

namespace MiniErp.Infrastructure.Persistence
{
    /// <summary>
    /// Usuario de la aplicación (Identity). Se puede ampliar con más campos de perfil.
    /// </summary>
    public class AppUser : IdentityUser<Guid>
    {
        /// <summary>Nombre visible para UI/claims.</summary>
        public string DisplayName { get; set; } = string.Empty;
    }
}
