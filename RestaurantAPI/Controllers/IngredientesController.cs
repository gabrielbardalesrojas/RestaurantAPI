using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.Models;
using RestaurantAPI.Models.DTOs;

namespace RestaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientesController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public IngredientesController(RestaurantDbContext context)
        {
            _context = context;
        }

        // GET: api/Ingredientes
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<IngredienteDto>>> GetIngredientes()
        {
            var ingredientes = await _context.Ingredientes
                .Where(i => i.Activo)
                .Select(i => new IngredienteDto
                {
                    Id = i.Id,
                    Nombre = i.Nombre,
                    Descripcion = i.Descripcion,
                    Alergeno = i.Alergeno,
                    Activo = i.Activo
                })
                .ToListAsync();

            return Ok(ingredientes);
        }

        // GET: api/Ingredientes/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IngredienteDto>> GetIngrediente(int id)
        {
            var ingrediente = await _context.Ingredientes
                .Where(i => i.Id == id)
                .Select(i => new IngredienteDto
                {
                    Id = i.Id,
                    Nombre = i.Nombre,
                    Descripcion = i.Descripcion,
                    Alergeno = i.Alergeno,
                    Activo = i.Activo
                })
                .FirstOrDefaultAsync();

            if (ingrediente == null)
                return NotFound(new { message = "Ingrediente no encontrado" });

            return Ok(ingrediente);
        }

        // GET: api/Ingredientes/Producto/5
        [HttpGet("Producto/{productoId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<IngredienteSimpleDto>>> GetIngredientesPorProducto(int productoId)
        {
            var ingredientes = await _context.ProductoIngredientes
                .Include(pi => pi.Ingrediente)
                .Where(pi => pi.ProductoId == productoId && pi.Ingrediente.Activo)
                .Select(pi => new IngredienteSimpleDto
                {
                    Id = pi.Ingrediente.Id,
                    Nombre = pi.Ingrediente.Nombre,
                    Cantidad = pi.Cantidad,
                    Alergeno = pi.Ingrediente.Alergeno
                })
                .ToListAsync();

            return Ok(ingredientes);
        }

        // GET: api/Ingredientes/Alergenos
        [HttpGet("Alergenos")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<IngredienteDto>>> GetAlergenos()
        {
            var alergenos = await _context.Ingredientes
                .Where(i => i.Alergeno && i.Activo)
                .Select(i => new IngredienteDto
                {
                    Id = i.Id,
                    Nombre = i.Nombre,
                    Descripcion = i.Descripcion,
                    Alergeno = i.Alergeno,
                    Activo = i.Activo
                })
                .ToListAsync();

            return Ok(alergenos);
        }

        // POST: api/Ingredientes
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IngredienteDto>> PostIngrediente(IngredienteDto dto)
        {
            // Verificar si el ingrediente ya existe
            var existe = await _context.Ingredientes
                .AnyAsync(i => i.Nombre.ToLower() == dto.Nombre.ToLower());

            if (existe)
                return BadRequest(new { message = "Ya existe un ingrediente con ese nombre" });

            var ingrediente = new Ingrediente
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Alergeno = dto.Alergeno
            };

            _context.Ingredientes.Add(ingrediente);
            await _context.SaveChangesAsync();

            dto.Id = ingrediente.Id;
            dto.Activo = ingrediente.Activo;

            return CreatedAtAction(nameof(GetIngrediente), new { id = ingrediente.Id }, dto);
        }

        // PUT: api/Ingredientes/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutIngrediente(int id, IngredienteDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "El ID no coincide" });

            var ingrediente = await _context.Ingredientes.FindAsync(id);
            if (ingrediente == null)
                return NotFound(new { message = "Ingrediente no encontrado" });

            // Verificar si otro ingrediente ya tiene ese nombre
            var existe = await _context.Ingredientes
                .AnyAsync(i => i.Nombre.ToLower() == dto.Nombre.ToLower() && i.Id != id);

            if (existe)
                return BadRequest(new { message = "Ya existe otro ingrediente con ese nombre" });

            ingrediente.Nombre = dto.Nombre;
            ingrediente.Descripcion = dto.Descripcion;
            ingrediente.Alergeno = dto.Alergeno;
            ingrediente.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Ingredientes/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteIngrediente(int id)
        {
            var ingrediente = await _context.Ingredientes.FindAsync(id);
            if (ingrediente == null)
                return NotFound(new { message = "Ingrediente no encontrado" });

            // Verificar si está asociado a algún producto
            var tieneProductos = await _context.ProductoIngredientes
                .AnyAsync(pi => pi.IngredienteId == id);

            if (tieneProductos)
                return BadRequest(new { message = "No se puede eliminar el ingrediente porque está asociado a productos" });

            ingrediente.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Ingredientes/AsignarAProducto
        [HttpPost("AsignarAProducto")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AsignarIngredienteAProducto(AsignarIngredienteDto dto)
        {
            // Verificar que el producto exista
            var producto = await _context.Productos.FindAsync(dto.ProductoId);
            if (producto == null)
                return NotFound(new { message = "Producto no encontrado" });

            // Verificar que el ingrediente exista
            var ingrediente = await _context.Ingredientes.FindAsync(dto.IngredienteId);
            if (ingrediente == null)
                return NotFound(new { message = "Ingrediente no encontrado" });

            // Verificar si ya está asignado
            var yaAsignado = await _context.ProductoIngredientes
                .AnyAsync(pi => pi.ProductoId == dto.ProductoId && pi.IngredienteId == dto.IngredienteId);

            if (yaAsignado)
                return BadRequest(new { message = "El ingrediente ya está asignado a este producto" });

            // Crear la relación
            var productoIngrediente = new ProductoIngrediente
            {
                ProductoId = dto.ProductoId,
                IngredienteId = dto.IngredienteId,
                Cantidad = dto.Cantidad
            };

            _context.ProductoIngredientes.Add(productoIngrediente);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ingrediente asignado correctamente al producto" });
        }

        // DELETE: api/Ingredientes/RemoverDeProducto/5
        [HttpDelete("RemoverDeProducto/{productoIngredienteId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoverIngredienteDeProducto(int productoIngredienteId)
        {
            var productoIngrediente = await _context.ProductoIngredientes
                .FindAsync(productoIngredienteId);

            if (productoIngrediente == null)
                return NotFound(new { message = "Relación no encontrada" });

            _context.ProductoIngredientes.Remove(productoIngrediente);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ingrediente removido del producto correctamente" });
        }

        // PUT: api/Ingredientes/ActualizarCantidad/5
        [HttpPut("ActualizarCantidad/{productoIngredienteId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActualizarCantidadIngrediente(int productoIngredienteId, [FromBody] string nuevaCantidad)
        {
            if (string.IsNullOrWhiteSpace(nuevaCantidad))
                return BadRequest(new { message = "La cantidad no puede estar vacía" });

            var productoIngrediente = await _context.ProductoIngredientes
                .FindAsync(productoIngredienteId);

            if (productoIngrediente == null)
                return NotFound(new { message = "Relación no encontrada" });

            productoIngrediente.Cantidad = nuevaCantidad;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cantidad actualizada correctamente" });
        }

        // GET: api/Ingredientes/BuscarPorNombre/{nombre}
        [HttpGet("BuscarPorNombre/{nombre}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<IngredienteDto>>> BuscarPorNombre(string nombre)
        {
            var ingredientes = await _context.Ingredientes
                .Where(i => i.Nombre.Contains(nombre) && i.Activo)
                .Select(i => new IngredienteDto
                {
                    Id = i.Id,
                    Nombre = i.Nombre,
                    Descripcion = i.Descripcion,
                    Alergeno = i.Alergeno,
                    Activo = i.Activo
                })
                .ToListAsync();

            return Ok(ingredientes);
        }


        // DELETE: api/Ingredientes/RemoverDeProducto/{productoId}/{ingredienteId}
        [HttpDelete("RemoverDeProducto/{productoId}/{ingredienteId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoverIngredienteDeProductoPorIds(int productoId, int ingredienteId)
        {
            var productoIngrediente = await _context.ProductoIngredientes
                .FirstOrDefaultAsync(pi => pi.ProductoId == productoId && pi.IngredienteId == ingredienteId);

            if (productoIngrediente == null)
                return NotFound(new { message = "Relación no encontrada" });

            _context.ProductoIngredientes.Remove(productoIngrediente);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ingrediente removido del producto correctamente" });
        }
    }
}