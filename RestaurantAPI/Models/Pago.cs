using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAPI.Models
{
    public class Pago
    {
        [Key]
        public int Id { get; set; }

        public int PedidoId { get; set; }

        public int MedioPagoId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        public DateTime FechaHoraPago { get; set; } = DateTime.Now;

        [MaxLength(450)]
        public string? UsuarioCajeroId { get; set; }

        [MaxLength(500)]
        public string Observaciones { get; set; }

        // Navegación
        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; }

        [ForeignKey("MedioPagoId")]
        public virtual MedioPago MedioPago { get; set; }

        [ForeignKey("UsuarioCajeroId")]
        public virtual Usuario UsuarioCajero { get; set; }
    }
}