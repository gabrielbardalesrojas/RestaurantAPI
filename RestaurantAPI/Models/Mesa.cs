using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models
{
    public class Mesa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string NumeroMesa { get; set; }

        [Required]
        [MaxLength(8)]
        public string CodigoMesa { get; set; } // Código único de 8 dígitos

        public int Capacidad { get; set; }

        public bool Disponible { get; set; } = true;

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navegación
        public virtual ICollection<Pedido> Pedidos { get; set; }
    }
}