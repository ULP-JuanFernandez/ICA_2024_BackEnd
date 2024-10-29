using System.ComponentModel.DataAnnotations;

namespace ICA.Models
{
    public class UsuarioEditViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El apellido no puede tener más de 100 caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [StringLength(255, ErrorMessage = "El correo no puede tener más de 255 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio.")]
        [StringLength(50, ErrorMessage = "El rol no puede tener más de 50 caracteres.")]
        [RegularExpression(@"^(invitado|usuario|admin)$", ErrorMessage = "El rol debe ser 'invitado', 'usuario' o 'admin'.")]
        public string Rol { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime FechaModificacion { get; set; } = DateTime.UtcNow; // Setter privado

        public byte Estado { get; set; } = 1;

        // Método para actualizar la fecha de modificación
        public void ActualizarFechaModificacion()
        {
            FechaModificacion = DateTime.UtcNow;
        }
    }
}
