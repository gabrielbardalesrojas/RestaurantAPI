using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models.DTOs
{
    public class MedioPagoDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Descripcion { get; set; }

        public bool Activo { get; set; }
    }

    public class PagoDto
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int MedioPagoId { get; set; }
        public string MedioPagoNombre { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaHoraPago { get; set; }
        public string UsuarioCajero { get; set; }
        public string Observaciones { get; set; }
    }

    public class ProcesarPagoDto
    {
        [Required(ErrorMessage = "El ID del pedido es obligatorio")]
        public int PedidoId { get; set; }

        [Required(ErrorMessage = "El ID del medio de pago es obligatorio")]
        public int MedioPagoId { get; set; }

        [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string Observaciones { get; set; }
    }
}