using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models.DTOs
{
    public class IngredienteDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Descripcion { get; set; }

        public bool Alergeno { get; set; }

        public bool Activo { get; set; }
    }

    public class IngredienteSimpleDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Cantidad { get; set; }
        public bool Alergeno { get; set; }
    }

    public class AsignarIngredienteDto
    {
        [Required(ErrorMessage = "El ID del producto es obligatorio")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El ID del ingrediente es obligatorio")]
        public int IngredienteId { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [MaxLength(50, ErrorMessage = "La cantidad no puede exceder 50 caracteres")]
        public string Cantidad { get; set; }
    }
}