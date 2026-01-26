
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniErp.Infrastructure.Persistence;

namespace MiniErp.Infrastructure
{
    /// <summary>Inicializa roles y un usuario admin por defecto.</summary>
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider sp)
        {
            var context = sp.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync(); // Aplica migraciones al iniciar

            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userMgr = sp.GetRequiredService<UserManager<AppUser>>();

            string[] roles = { "Admin", "Manager", "User" };
            foreach (var r in roles)
            {
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole<Guid>(r));
            }

            var adminEmail = "admin@minierp.local";
            var admin = await userMgr.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (admin == null)
            {
                admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    DisplayName = "Super Admin"
                };

                // Nota: cambia la contraseña en producción
                var created = await userMgr.CreateAsync(admin, "Admin123$");
                if (created.Succeeded)
                {
                    await userMgr.AddToRoleAsync(admin, "Admin");
                }
                else
                {
                    throw new InvalidOperationException("No fue posible crear el usuario administrador por defecto.");
                }
            }
        }
    }
}
