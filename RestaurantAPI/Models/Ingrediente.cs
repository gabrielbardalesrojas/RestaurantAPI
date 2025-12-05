using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models
{
    public class Ingrediente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(500)]
        public string Descripcion { get; set; }

        public bool Alergeno { get; set; } = false; // Si es alérgeno

        public bool Activo { get; set; } = true;

        // Navegación
        public virtual ICollection<ProductoIngrediente> ProductoIngredientes { get; set; }
    }
}