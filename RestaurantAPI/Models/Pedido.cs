using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAPI.Models
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string NumeroPedido { get; set; } // Ej: PED-20241203-001

        public int MesaId { get; set; }

        [MaxLength(450)]
        public string? UsuarioCreadorId { get; set; } // Cliente o Mesero que creó el pedido

        public DateTime FechaHoraPedido { get; set; } = DateTime.Now;

        public DateTime? FechaHoraCompletado { get; set; }

        public DateTime? FechaHoraCancelado { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, EnPreparacion, Completado, Cancelado

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [MaxLength(500)]
        public string Observaciones { get; set; }

        // Navegación
        [ForeignKey("MesaId")]
        public virtual Mesa Mesa { get; set; }

        [ForeignKey("UsuarioCreadorId")]
        public virtual Usuario UsuarioCreador { get; set; }

        public virtual ICollection<DetallePedido> DetallesPedido { get; set; } = new List<DetallePedido>();

        public virtual Pago Pago { get; set; }
    }
}