using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.Models;
using RestaurantAPI.Models.DTOs;
using System.Security.Claims;

namespace RestaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public PedidosController(RestaurantDbContext context)
        {
            _context = context;
        }

        // CLIENTE: Crear pedido desde la mesa
        [HttpPost("cliente")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<PedidoDto>> CrearPedidoCliente(CrearPedidoDto dto)
        {
            return await CrearPedido(dto);
        }

        // MESERO: Crear pedido
        [HttpPost("mesero")]
        [Authorize(Roles = "Mesero,Admin")]
        public async Task<ActionResult<PedidoDto>> CrearPedidoMesero(CrearPedidoDto dto)
        {
            return await CrearPedido(dto);
        }

        private async Task<ActionResult<PedidoDto>> CrearPedido(CrearPedidoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Generar número de pedido
            var fecha = DateTime.Now.ToString("yyyyMMdd");
            var ultimoPedido = await _context.Pedidos
                .Where(p => p.NumeroPedido.StartsWith($"PED-{fecha}"))
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            int siguiente = 1;
            if (ultimoPedido != null)
            {
                var partes = ultimoPedido.NumeroPedido.Split('-');
                if (partes.Length == 3 && int.TryParse(partes[2], out int ultimo))
                {
                    siguiente = ultimo + 1;
                }
            }

            var numeroPedido = $"PED-{fecha}-{siguiente:D3}";

            // Crear pedido
            var pedido = new Pedido
            {
                NumeroPedido = numeroPedido,
                MesaId = dto.MesaId,
                UsuarioCreadorId = userId,
                Observaciones = dto.Observaciones,
                Estado = "Pendiente"
            };

            decimal total = 0;

            // Agregar detalles
            foreach (var detalle in dto.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                if (producto == null || !producto.Disponible)
                    return BadRequest(new { message = $"Producto {detalle.ProductoId} no disponible" });

                var subtotal = producto.Precio * detalle.Cantidad;
                total += subtotal;

                pedido.DetallesPedido.Add(new DetallePedido
                {
                    ProductoId = detalle.ProductoId,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = producto.Precio,
                    Subtotal = subtotal,
                    Observaciones = detalle.Observaciones
                });
            }

            pedido.Total = total;

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            return await ObtenerPedidoDto(pedido.Id);
        }

        // MESERO: Listar pedidos pendientes
        [HttpGet("mesero/pendientes")]
        [Authorize(Roles = "Mesero,Admin")]
        public async Task<ActionResult<IEnumerable<PedidoDto>>> GetPedidosPendientesMesero()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.UsuarioCreador)
                .Include(p => p.DetallesPedido)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.Estado == "Pendiente" || p.Estado == "EnPreparacion")
                .OrderBy(p => p.FechaHoraPedido)
                .Select(p => new PedidoDto
                {
                    Id = p.Id,
                    NumeroPedido = p.NumeroPedido,
                    MesaId = p.MesaId,
                    NumeroMesa = p.Mesa.NumeroMesa,
                    UsuarioCreador = p.UsuarioCreador != null ? p.UsuarioCreador.NombreCompleto : "Cliente",
                    FechaHoraPedido = p.FechaHoraPedido,
                    Estado = p.Estado,
                    Total = p.Total,
                    Observaciones = p.Observaciones,
                    Detalles = p.DetallesPedido.Select(d => new DetallePedidoDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal,
                        Observaciones = d.Observaciones,
                        Completado = d.Completado
                    }).ToList()
                })
                .ToListAsync();

            return Ok(pedidos);
        }

        // MESERO: Actualizar pedido
        [HttpPut("{id}/mesero")]
        [Authorize(Roles = "Mesero,Admin")]
        public async Task<IActionResult> ActualizarPedidoMesero(int id, ActualizarPedidoDto dto)
        {
            if (id != dto.PedidoId)
                return BadRequest();

            var pedido = await _context.Pedidos
                .Include(p => p.DetallesPedido)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
                return NotFound();

            if (pedido.Estado != "Pendiente")
                return BadRequest(new { message = "Solo se pueden editar pedidos pendientes" });

            // Eliminar detalles anteriores
            _context.DetallesPedido.RemoveRange(pedido.DetallesPedido);

            // Agregar nuevos detalles
            decimal total = 0;
            foreach (var detalle in dto.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                if (producto == null || !producto.Disponible)
                    return BadRequest(new { message = $"Producto {detalle.ProductoId} no disponible" });

                var subtotal = producto.Precio * detalle.Cantidad;
                total += subtotal;

                pedido.DetallesPedido.Add(new DetallePedido
                {
                    ProductoId = detalle.ProductoId,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = producto.Precio,
                    Subtotal = subtotal,
                    Observaciones = detalle.Observaciones
                });
            }

            pedido.Total = total;
            pedido.Observaciones = dto.Observaciones;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // COCINERO: Listar pedidos para cocina
        [HttpGet("cocinero/pendientes")]
        [Authorize(Roles = "Cocinero,Admin")]
        public async Task<ActionResult<IEnumerable<PedidoDto>>> GetPedidosCocinero()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.DetallesPedido)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.Estado == "Pendiente" || p.Estado == "EnPreparacion")
                .OrderBy(p => p.FechaHoraPedido)
                .Select(p => new PedidoDto
                {
                    Id = p.Id,
                    NumeroPedido = p.NumeroPedido,
                    MesaId = p.MesaId,
                    NumeroMesa = p.Mesa.NumeroMesa,
                    FechaHoraPedido = p.FechaHoraPedido,
                    Estado = p.Estado,
                    Total = p.Total,
                    Observaciones = p.Observaciones,
                    Detalles = p.DetallesPedido.Select(d => new DetallePedidoDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal,
                        Observaciones = d.Observaciones,
                        Completado = d.Completado
                    }).ToList()
                })
                .ToListAsync();

            return Ok(pedidos);
        }

        // COCINERO: Marcar detalle como completado
        [HttpPut("cocinero/marcar-detalle")]
        [Authorize(Roles = "Cocinero,Admin")]
        public async Task<IActionResult> MarcarDetalleCompletado(MarcarDetalleCompletadoDto dto)
        {
            var detalle = await _context.DetallesPedido
                .Include(d => d.Pedido)
                    .ThenInclude(p => p.DetallesPedido)
                .FirstOrDefaultAsync(d => d.Id == dto.DetallePedidoId);

            if (detalle == null)
                return NotFound();

            detalle.Completado = dto.Completado;
            if (dto.Completado)
            {
                detalle.FechaHoraCompletado = DateTime.Now;
            }

            // Verificar si todos los detalles están completados
            var todosCompletados = detalle.Pedido.DetallesPedido.All(d => d.Completado || d.Id == detalle.Id && dto.Completado);

            if (todosCompletados)
            {
                detalle.Pedido.Estado = "Completado";
                detalle.Pedido.FechaHoraCompletado = DateTime.Now;
            }
            else if (detalle.Pedido.Estado == "Pendiente")
            {
                detalle.Pedido.Estado = "EnPreparacion";
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // CAJERO: Listar pedidos completados
        [HttpGet("cajero/completados")]
        [Authorize(Roles = "Cajero,Admin")]
        public async Task<ActionResult<IEnumerable<PedidoDto>>> GetPedidosCompletados()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.DetallesPedido)
                    .ThenInclude(d => d.Producto)
                .Include(p => p.Pago)
                .Where(p => p.Estado == "Completado" || p.Estado == "Cancelado")
                .OrderByDescending(p => p.Estado == "Completado")
                .ThenBy(p => p.FechaHoraCompletado)
                .Select(p => new PedidoDto
                {
                    Id = p.Id,
                    NumeroPedido = p.NumeroPedido,
                    MesaId = p.MesaId,
                    NumeroMesa = p.Mesa.NumeroMesa,
                    FechaHoraPedido = p.FechaHoraPedido,
                    Estado = p.Estado,
                    Total = p.Total,
                    Observaciones = p.Observaciones,
                    Detalles = p.DetallesPedido.Select(d => new DetallePedidoDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal,
                        Observaciones = d.Observaciones,
                        Completado = d.Completado
                    }).ToList()
                })
                .ToListAsync();

            return Ok(pedidos);
        }

        // ADMIN: Reporte de pedidos por día
        [HttpGet("admin/reporte/{fecha}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ReportePedidosDto>> GetReportePorDia(DateTime fecha)
        {
            var inicioDia = fecha.Date;
            var finDia = inicioDia.AddDays(1);

            var pedidos = await _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.DetallesPedido)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.FechaHoraPedido >= inicioDia && p.FechaHoraPedido < finDia)
                .ToListAsync();

            var reporte = new ReportePedidosDto
            {
                Fecha = fecha,
                TotalPedidos = pedidos.Count,
                PedidosPendientes = pedidos.Count(p => p.Estado == "Pendiente" || p.Estado == "EnPreparacion"),
                PedidosCompletados = pedidos.Count(p => p.Estado == "Completado"),
                PedidosCancelados = pedidos.Count(p => p.Estado == "Cancelado"),
                TotalVentas = pedidos.Where(p => p.Estado == "Cancelado").Sum(p => p.Total),
                Pedidos = pedidos.Select(p => new PedidoDto
                {
                    Id = p.Id,
                    NumeroPedido = p.NumeroPedido,
                    NumeroMesa = p.Mesa.NumeroMesa,
                    FechaHoraPedido = p.FechaHoraPedido,
                    Estado = p.Estado,
                    Total = p.Total,
                    Detalles = p.DetallesPedido.Select(d => new DetallePedidoDto
                    {
                        ProductoNombre = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    }).ToList()
                }).ToList()
            };

            return Ok(reporte);
        }

        private async Task<ActionResult<PedidoDto>> ObtenerPedidoDto(int pedidoId)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Mesa)
                .Include(p => p.UsuarioCreador)
                .Include(p => p.DetallesPedido)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.Id == pedidoId);

            if (pedido == null)
                return NotFound();

            return Ok(new PedidoDto
            {
                Id = pedido.Id,
                NumeroPedido = pedido.NumeroPedido,
                MesaId = pedido.MesaId,
                NumeroMesa = pedido.Mesa.NumeroMesa,
                UsuarioCreador = pedido.UsuarioCreador?.NombreCompleto ?? "Cliente",
                FechaHoraPedido = pedido.FechaHoraPedido,
                Estado = pedido.Estado,
                Total = pedido.Total,
                Observaciones = pedido.Observaciones,
                Detalles = pedido.DetallesPedido.Select(d => new DetallePedidoDto
                {
                    Id = d.Id,
                    ProductoId = d.ProductoId,
                    ProductoNombre = d.Producto.Nombre,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal,
                    Observaciones = d.Observaciones,
                    Completado = d.Completado
                }).ToList()
            });
        }
    }
}