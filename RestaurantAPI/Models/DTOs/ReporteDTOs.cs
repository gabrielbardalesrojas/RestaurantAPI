namespace RestaurantAPI.Models.DTOs
{
    public class ReportePedidosDto
    {
        public DateTime Fecha { get; set; }
        public int TotalPedidos { get; set; }
        public int PedidosPendientes { get; set; }
        public int PedidosCompletados { get; set; }
        public int PedidosCancelados { get; set; }
        public decimal TotalVentas { get; set; }
        public List<PedidoDto> Pedidos { get; set; }
    }
}