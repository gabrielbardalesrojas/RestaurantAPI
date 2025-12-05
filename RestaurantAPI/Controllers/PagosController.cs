using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.Models;
using RestaurantAPI.Models.DTOs;
using System.Security.Claims;

namespace RestaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Cajero,Admin")]
    public class PagosController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public PagosController(RestaurantDbContext context)
        {
            _context = context;
        }

        // Obtener medios de pago disponibles
        [HttpGet("medios-pago")]
        public async Task<ActionResult<IEnumerable<MedioPagoDto>>> GetMediosPago()
        {
            var medios = await _context.MediosPago
                .Where(m => m.Activo)
                .Select(m => new MedioPagoDto
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    Descripcion = m.Descripcion,
                    Activo = m.Activo
                })
                .ToListAsync();

            return Ok(medios);
        }

        // Procesar pago de un pedido
        [HttpPost("procesar")]
        public async Task<ActionResult<PagoDto>> ProcesarPago(ProcesarPagoDto dto)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Pago)
                .FirstOrDefaultAsync(p => p.Id == dto.PedidoId);

            if (pedido == null)
                return NotFound(new { message = "Pedido no encontrado" });

            if (pedido.Estado != "Completado")
                return BadRequest(new { message = "El pedido debe estar completado para procesar el pago" });

            if (pedido.Pago != null)
                return BadRequest(new { message = "Este pedido ya tiene un pago registrado" });

            var medioPago = await _context.MediosPago.FindAsync(dto.MedioPagoId);
            if (medioPago == null || !medioPago.Activo)
                return BadRequest(new { message = "Medio de pago no válido" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pago = new Pago
            {
                PedidoId = dto.PedidoId,
                MedioPagoId = dto.MedioPagoId,
                Monto = pedido.Total,
                UsuarioCajeroId = userId,
                Observaciones = dto.Observaciones
            };

            _context.Pagos.Add(pago);

            // Cambiar estado del pedido a Cancelado (Pagado)
            pedido.Estado = "Cancelado";
            pedido.FechaHoraCancelado = DateTime.Now;

            await _context.SaveChangesAsync();

            var usuario = await _context.Users.FindAsync(userId);

            return Ok(new PagoDto
            {
                Id = pago.Id,
                PedidoId = pago.PedidoId,
                MedioPagoId = pago.MedioPagoId,
                MedioPagoNombre = medioPago.Nombre,
                Monto = pago.Monto,
                FechaHoraPago = pago.FechaHoraPago,
                UsuarioCajero = usuario?.NombreCompleto,
                Observaciones = pago.Observaciones
            });
        }

        // Obtener pago de un pedido
        [HttpGet("pedido/{pedidoId}")]
        public async Task<ActionResult<PagoDto>> GetPagoPorPedido(int pedidoId)
        {
            var pago = await _context.Pagos
                .Include(p => p.MedioPago)
                .Include(p => p.UsuarioCajero)
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

            if (pago == null)
                return NotFound();

            return Ok(new PagoDto
            {
                Id = pago.Id,
                PedidoId = pago.PedidoId,
                MedioPagoId = pago.MedioPagoId,
                MedioPagoNombre = pago.MedioPago.Nombre,
                Monto = pago.Monto,
                FechaHoraPago = pago.FechaHoraPago,
                UsuarioCajero = pago.UsuarioCajero?.NombreCompleto,
                Observaciones = pago.Observaciones
            });
        }

        // Listar todos los pagos del día
        [HttpGet("dia/{fecha}")]
        public async Task<ActionResult<IEnumerable<PagoDto>>> GetPagosPorDia(DateTime fecha)
        {
            var inicioDia = fecha.Date;
            var finDia = inicioDia.AddDays(1);

            var pagos = await _context.Pagos
                .Include(p => p.MedioPago)
                .Include(p => p.UsuarioCajero)
                .Include(p => p.Pedido)
                    .ThenInclude(ped => ped.Mesa)
                .Where(p => p.FechaHoraPago >= inicioDia && p.FechaHoraPago < finDia)
                .Select(p => new PagoDto
                {
                    Id = p.Id,
                    PedidoId = p.PedidoId,
                    MedioPagoId = p.MedioPagoId,
                    MedioPagoNombre = p.MedioPago.Nombre,
                    Monto = p.Monto,
                    FechaHoraPago = p.FechaHoraPago,
                    UsuarioCajero = p.UsuarioCajero != null ? p.UsuarioCajero.NombreCompleto : "N/A",
                    Observaciones = p.Observaciones
                })
                .ToListAsync();

            return Ok(pagos);
        }
    }
}