using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAPI.Models
{
    public class ProductoIngrediente
    {
        [Key]
        public int Id { get; set; }

        public int ProductoId { get; set; }

        public int IngredienteId { get; set; }

        [MaxLength(50)]
        public string Cantidad { get; set; } // Ej: "200g", "2 unidades"

        // Navegación
        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }

        [ForeignKey("IngredienteId")]
        public virtual Ingrediente Ingrediente { get; set; }
    }
}