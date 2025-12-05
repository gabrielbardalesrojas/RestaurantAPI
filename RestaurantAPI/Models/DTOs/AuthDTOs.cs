using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El código es obligatorio")]
        public string Codigo { get; set; } // Puede ser código de mesa o código de empleado
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string TipoUsuario { get; set; } // Cliente, Mesero, Cocinero, Cajero, Admin
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public int? MesaId { get; set; } // Solo si es cliente
        public string NumeroMesa { get; set; }
    }

    public class RegisterEmpleadoDto
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        public string NombreCompleto { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; } // Mesero, Cocinero, Cajero, Admin
    }
}