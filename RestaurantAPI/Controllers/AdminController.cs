using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;

namespace RestaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;

        public AdminController(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _userManager.Users.ToListAsync();

            var result = new List<object>();

            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);

                result.Add(new
                {
                    id = usuario.Id,
                    nombreCompleto = usuario.NombreCompleto,
                    email = usuario.Email,
                    codigoEmpleado = usuario.CodigoEmpleado,
                    activo = usuario.Activo,
                    roles = roles
                });
            }

            return Ok(result);
        }
    }
}