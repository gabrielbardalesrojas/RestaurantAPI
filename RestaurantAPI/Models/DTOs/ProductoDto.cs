using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; }

        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string Descripcion { get; set; }

        [MaxLength(500, ErrorMessage = "La URL de la imagen no puede exceder 500 caracteres")]
        public string UrlImagen { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        public bool Disponible { get; set; }

        [Range(0, 300, ErrorMessage = "El tiempo de preparación debe estar entre 0 y 300 minutos")]
        public int TiempoPreparacion { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        public int CategoriaId { get; set; }

        public string CategoriaNombre { get; set; }

        public List<IngredienteSimpleDto> Ingredientes { get; set; }
    }
}