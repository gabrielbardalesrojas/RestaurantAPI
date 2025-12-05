using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Data;
using RestaurantAPI.Models;
using RestaurantAPI.Models.DTOs;

namespace RestaurantAPI.Controllers
{

    // ==================== PRODUCTOS CONTROLLER ====================
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly RestaurantDbContext _context;

        public ProductosController(RestaurantDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.ProductoIngredientes)
                    .ThenInclude(pi => pi.Ingrediente)
                .Where(p => p.Activo && p.Disponible)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    UrlImagen = p.UrlImagen,
                    Precio = p.Precio,
                    Disponible = p.Disponible,
                    TiempoPreparacion = p.TiempoPreparacion,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria.Nombre,
                    Ingredientes = p.ProductoIngredientes.Select(pi => new IngredienteSimpleDto
                    {
                        Id = pi.Ingrediente.Id,
                        Nombre = pi.Ingrediente.Nombre,
                        Cantidad = pi.Cantidad,
                        Alergeno = pi.Ingrediente.Alergeno
                    }).ToList()
                })
                .ToListAsync();

            return Ok(productos);
        }

        [HttpGet("categoria/{categoriaId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductosPorCategoria(int categoriaId)
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.ProductoIngredientes)
                    .ThenInclude(pi => pi.Ingrediente)
                .Where(p => p.CategoriaId == categoriaId && p.Activo && p.Disponible)
                .Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    UrlImagen = p.UrlImagen,
                    Precio = p.Precio,
                    Disponible = p.Disponible,
                    TiempoPreparacion = p.TiempoPreparacion,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria.Nombre,
                    Ingredientes = p.ProductoIngredientes.Select(pi => new IngredienteSimpleDto
                    {
                        Id = pi.Ingrediente.Id,
                        Nombre = pi.Ingrediente.Nombre,
                        Cantidad = pi.Cantidad,
                        Alergeno = pi.Ingrediente.Alergeno
                    }).ToList()
                })
                .ToListAsync();

            return Ok(productos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductoDto>> PostProducto(ProductoDto dto)
        {
            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                UrlImagen = dto.UrlImagen,
                Precio = dto.Precio,
                TiempoPreparacion = dto.TiempoPreparacion,
                CategoriaId = dto.CategoriaId
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return Ok(dto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutProducto(int id, ProductoDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            producto.Nombre = dto.Nombre;
            producto.Descripcion = dto.Descripcion;
            producto.UrlImagen = dto.UrlImagen;
            producto.Precio = dto.Precio;
            producto.Disponible = dto.Disponible;
            producto.TiempoPreparacion = dto.TiempoPreparacion;
            producto.CategoriaId = dto.CategoriaId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            producto.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}