using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.Models;
using RestaurantAPI.Models.DTOs;

namespace RestaurantAPI.Controllers
{
    // ==================== MESAS CONTROLLER ====================
    [Route("api/[controller]")]
    [ApiController]
    public class MesasController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public MesasController(RestaurantDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Mesero")]
        public async Task<ActionResult<IEnumerable<MesaDto>>> GetMesas()
        {
            var mesas = await _context.Mesas
                .Select(m => new MesaDto
                {
                    Id = m.Id,
                    NumeroMesa = m.NumeroMesa,
                    CodigoMesa = m.CodigoMesa,
                    Capacidad = m.Capacidad,
                    Disponible = m.Disponible,
                    Activo = m.Activo
                })
                .ToListAsync();

            return Ok(mesas);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MesaDto>> PostMesa(CrearMesaDto dto)
        {
            var codigoMesa = SeedData.GenerarCodigo();

            var mesa = new Mesa
            {
                NumeroMesa = dto.NumeroMesa,
                CodigoMesa = codigoMesa,
                Capacidad = dto.Capacidad
            };

            _context.Mesas.Add(mesa);
            await _context.SaveChangesAsync();

            return Ok(new MesaDto
            {
                Id = mesa.Id,
                NumeroMesa = mesa.NumeroMesa,
                CodigoMesa = mesa.CodigoMesa,
                Capacidad = mesa.Capacidad,
                Disponible = mesa.Disponible,
                Activo = mesa.Activo
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutMesa(int id, MesaDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var mesa = await _context.Mesas.FindAsync(id);
            if (mesa == null)
                return NotFound();

            mesa.NumeroMesa = dto.NumeroMesa;
            mesa.Capacidad = dto.Capacidad;
            mesa.Disponible = dto.Disponible;
            mesa.Activo = dto.Activo;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMesa(int id)
        {
            var mesa = await _context.Mesas.FindAsync(id);
            if (mesa == null)
                return NotFound();

            mesa.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}