using System.ComponentModel.DataAnnotations;

namespace ICA.Models
{
    public class UsuarioLoginViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Clave { get; set; } = string.Empty;

        public bool Recordarme { get; set; }
    }
}
