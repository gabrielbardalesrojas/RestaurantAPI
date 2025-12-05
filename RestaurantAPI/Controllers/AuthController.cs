using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestaurantAPI.Data;
using RestaurantAPI.Models;
using RestaurantAPI.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RestaurantAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RestaurantDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<Usuario> userManager,
            RestaurantDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar si es código de mesa
            var mesa = await _context.Mesas
                .FirstOrDefaultAsync(m => m.CodigoMesa == model.Codigo && m.Activo);

            if (mesa != null)
            {
                // Login como cliente de mesa
                return await LoginCliente(mesa);
            }

            // Verificar si es código de empleado
            var empleado = await _userManager.Users
                .FirstOrDefaultAsync(u => u.CodigoEmpleado == model.Codigo && u.Activo);

            if (empleado != null)
            {
                // Login como empleado
                return await LoginEmpleado(empleado);
            }

            return Unauthorized(new { message = "Código inválido o no encontrado" });
        }

        private async Task<IActionResult> LoginCliente(Mesa mesa)
        {
            // Crear o buscar usuario temporal para esta mesa
            var clienteEmail = $"mesa{mesa.Id}@cliente.com";
            var cliente = await _userManager.FindByEmailAsync(clienteEmail);

            if (cliente == null)
            {
                cliente = new Usuario
                {
                    UserName = clienteEmail,
                    Email = clienteEmail,
                    NombreCompleto = $"Cliente Mesa {mesa.NumeroMesa}",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(cliente, "Cliente123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(cliente, "Cliente");
                }
            }

            var roles = new List<string> { "Cliente" };
            var token = GenerateJwtToken(cliente, roles);

            return Ok(new AuthResponseDto
            {
                Token = token,
                TipoUsuario = "Cliente",
                NombreCompleto = cliente.NombreCompleto,
                Email = cliente.Email,
                Roles = roles,
                MesaId = mesa.Id,
                NumeroMesa = mesa.NumeroMesa
            });
        }

        private async Task<IActionResult> LoginEmpleado(Usuario empleado)
        {
            var roles = (await _userManager.GetRolesAsync(empleado)).ToList();
            var token = GenerateJwtToken(empleado, roles);

            string tipoUsuario = roles.FirstOrDefault() ?? "Usuario";

            return Ok(new AuthResponseDto
            {
                Token = token,
                TipoUsuario = tipoUsuario,
                NombreCompleto = empleado.NombreCompleto,
                Email = empleado.Email,
                Roles = roles
            });
        }

        [HttpPost("register-empleado")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterEmpleado([FromBody] RegisterEmpleadoDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var codigoEmpleado = SeedData.GenerarCodigo();

            var usuario = new Usuario
            {
                UserName = model.Email,
                Email = model.Email,
                NombreCompleto = model.NombreCompleto,
                CodigoEmpleado = codigoEmpleado,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(usuario, "Empleado123!");

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Validar rol
            var rolesValidos = new[] { "Mesero", "Cocinero", "Cajero", "Admin" };
            if (!rolesValidos.Contains(model.Rol))
                return BadRequest(new { message = "Rol inválido" });

            await _userManager.AddToRoleAsync(usuario, model.Rol);

            return Ok(new
            {
                message = "Empleado registrado exitosamente",
                codigoEmpleado = codigoEmpleado,
                email = model.Email,
                rol = model.Rol
            });
        }

        private string GenerateJwtToken(Usuario usuario, List<string> roles)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(
                    double.Parse(jwtSettings["ExpirationInHours"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}