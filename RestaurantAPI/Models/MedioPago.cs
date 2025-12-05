using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models
{
    public class MedioPago
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } // Efectivo, Tarjeta, Yape, Plin, etc

        [MaxLength(500)]
        public string Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        // Navegación
        public virtual ICollection<Pago> Pagos { get; set; }
    }
}