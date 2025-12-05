using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models.DTOs
{
    public class PedidoDto
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; }
        public int MesaId { get; set; }
        public string NumeroMesa { get; set; }
        public string UsuarioCreador { get; set; }
        public DateTime FechaHoraPedido { get; set; }
        public string Estado { get; set; }
        public decimal Total { get; set; }
        public string Observaciones { get; set; }
        public List<DetallePedidoDto> Detalles { get; set; }
    }

    public class CrearPedidoDto
    {
        [Required(ErrorMessage = "El ID de la mesa es obligatorio")]
        public int MesaId { get; set; }

        [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string Observaciones { get; set; }

        [Required(ErrorMessage = "Los detalles del pedido son obligatorios")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
        public List<CrearDetallePedidoDto> Detalles { get; set; }
    }

    public class ActualizarPedidoDto
    {
        [Required(ErrorMessage = "El ID del pedido es obligatorio")]
        public int PedidoId { get; set; }

        [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string Observaciones { get; set; }

        [Required(ErrorMessage = "Los detalles del pedido son obligatorios")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
        public List<CrearDetallePedidoDto> Detalles { get; set; }
    }

    public class DetallePedidoDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string Observaciones { get; set; }
        public bool Completado { get; set; }
    }

    public class CrearDetallePedidoDto
    {
        [Required(ErrorMessage = "El ID del producto es obligatorio")]
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, 100, ErrorMessage = "La cantidad debe estar entre 1 y 100")]
        public int Cantidad { get; set; }

        [MaxLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string Observaciones { get; set; }
    }

    public class MarcarDetalleCompletadoDto
    {
        [Required(ErrorMessage = "El ID del detalle del pedido es obligatorio")]
        public int DetallePedidoId { get; set; }

        [Required(ErrorMessage = "El estado completado es obligatorio")]
        public bool Completado { get; set; }
    }
}