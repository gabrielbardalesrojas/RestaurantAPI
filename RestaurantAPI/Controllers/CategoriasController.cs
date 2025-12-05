using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.Models;
using RestaurantAPI.Models.DTOs;

namespace RestaurantAPI.Controllers
{
    

    // ==================== CATEGORÍAS CONTROLLER ====================
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public CategoriasController(RestaurantDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoriaProductoDto>>> GetCategorias()
        {
            var categorias = await _context.CategoriasProducto
                .Where(c => c.Activo)
                .OrderBy(c => c.Orden)
                .Select(c => new CategoriaProductoDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Orden = c.Orden,
                    Activo = c.Activo
                })
                .ToListAsync();

            return Ok(categorias);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoriaProductoDto>> PostCategoria(CategoriaProductoDto dto)
        {
            var categoria = new CategoriaProducto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Orden = dto.Orden
            };

            _context.CategoriasProducto.Add(categoria);
            await _context.SaveChangesAsync();

            dto.Id = categoria.Id;
            return CreatedAtAction(nameof(GetCategorias), new { id = categoria.Id }, dto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCategoria(int id, CategoriaProductoDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var categoria = await _context.CategoriasProducto.FindAsync(id);
            if (categoria == null)
                return NotFound();

            categoria.Nombre = dto.Nombre;
            categoria.Descripcion = dto.Descripcion;
            categoria.Orden = dto.Orden;
            categoria.Activo = dto.Activo;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.CategoriasProducto.FindAsync(id);
            if (categoria == null)
                return NotFound();

            categoria.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}