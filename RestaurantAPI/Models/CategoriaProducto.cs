using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models
{
    public class CategoriaProducto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } // Comidas, Bebidas, Postres, Entradas

        [MaxLength(500)]
        public string Descripcion { get; set; }

        public int Orden { get; set; } // Para ordenar en el menú

        public bool Activo { get; set; } = true;

        // Navegación
        public virtual ICollection<Producto> Productos { get; set; }
    }
}