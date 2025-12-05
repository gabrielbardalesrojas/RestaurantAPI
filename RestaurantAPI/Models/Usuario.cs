using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models
{
    public class Usuario : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string NombreCompleto { get; set; }

        [MaxLength(8)]
        public string? CodigoEmpleado { get; set; } // Código para meseros, cocineros, cajeros

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;

        // Navegación
        public virtual ICollection<Pedido> PedidosCreados { get; set; }
    }
}