using Microsoft.AspNetCore.Identity;
using RestaurantAPI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantAPI.Data
{
    // Única definición de la clase SeedData
    public static class SeedData
    {
        // Única definición del método Initialize
        public static async Task Initialize(
            RoleManager<IdentityRole> roleManager,
            UserManager<Usuario> userManager,
            RestaurantDbContext context)
        {
            // Crear roles (Admin, Mesero, Cocinero, Cajero, Cliente)
            string[] roles = { "Admin", "Mesero", "Cocinero", "Cajero", "Cliente" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Crear usuario Admin por defecto
            var adminEmail = "admin@restaurant.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new Usuario
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NombreCompleto = "Administrador del Sistema",
                    CodigoEmpleado = GenerarCodigo(),
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin123!"); // Contraseña
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin"); // Asignación de rol
                }
            }

            // Crear medios de pago por defecto
            if (!context.MediosPago.Any())
            {
                context.MediosPago.AddRange(
                    new MedioPago { Nombre = "Efectivo", Descripcion = "Pago en efectivo" },
                    new MedioPago { Nombre = "Tarjeta", Descripcion = "Tarjeta de crédito/débito" },
                    new MedioPago { Nombre = "Yape", Descripcion = "Transferencia Yape" },
                    new MedioPago { Nombre = "Plin", Descripcion = "Transferencia Plin" }
                );
                await context.SaveChangesAsync();
            }

            // Crear categorías por defecto
            if (!context.CategoriasProducto.Any())
            {
                context.CategoriasProducto.AddRange(
                    new CategoriaProducto { Nombre = "Entradas", Descripcion = "Platos de entrada", Orden = 1 },
                    new CategoriaProducto { Nombre = "Platos Principales", Descripcion = "Platos fuertes", Orden = 2 },
                    new CategoriaProducto { Nombre = "Bebidas", Descripcion = "Todo tipo de bebidas", Orden = 3 },
                    new CategoriaProducto { Nombre = "Postres", Descripcion = "Dulces y postres", Orden = 4 }
                );
                await context.SaveChangesAsync();
            }
        }

        // Única definición del método GenerarCodigo
        public static string GenerarCodigo()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}