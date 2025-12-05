using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAPI.Models
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; }

        [MaxLength(1000)]
        public string Descripcion { get; set; }

        [MaxLength(500)]
        public string UrlImagen { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        public bool Disponible { get; set; } = true;

        public bool Activo { get; set; } = true;

        public int TiempoPreparacion { get; set; } // En minutos

        // Foreign Key
        [Required]
        public int CategoriaId { get; set; }

        // Navegación
        [ForeignKey("CategoriaId")]
        public virtual CategoriaProducto Categoria { get; set; }

        public virtual ICollection<ProductoIngrediente> ProductoIngredientes { get; set; }

        public virtual ICollection<DetallePedido> DetallesPedido { get; set; }
    }
}