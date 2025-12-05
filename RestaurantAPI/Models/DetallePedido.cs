using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAPI.Models
{
    public class DetallePedido
    {
        [Key]
        public int Id { get; set; }

        public int PedidoId { get; set; }

        public int ProductoId { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        [MaxLength(500)]
        public string Observaciones { get; set; }

        public bool Completado { get; set; } = false; // Para que el cocinero marque

        public DateTime? FechaHoraCompletado { get; set; }

        // Navegación
        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; }
    }
}