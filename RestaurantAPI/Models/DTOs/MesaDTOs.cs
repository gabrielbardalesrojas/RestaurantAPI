using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models.DTOs
{
    public class MesaDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El número de mesa es obligatorio")]
        public string NumeroMesa { get; set; }

        public string CodigoMesa { get; set; }

        public int Capacidad { get; set; }

        public bool Disponible { get; set; }

        public bool Activo { get; set; }
    }

    public class CrearMesaDto
    {
        [Required(ErrorMessage = "El número de mesa es obligatorio")]
        [MaxLength(50, ErrorMessage = "El número de mesa no puede exceder 50 caracteres")]
        public string NumeroMesa { get; set; }

        [Required(ErrorMessage = "La capacidad es obligatoria")]
        [Range(1, 20, ErrorMessage = "La capacidad debe estar entre 1 y 20")]
        public int Capacidad { get; set; }
    }
}