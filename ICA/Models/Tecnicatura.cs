using System.ComponentModel.DataAnnotations;

namespace ICA.Models
{
    public class Tecnicatura
    {
        [Key]
        [Display(Name = "Código")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El campo Nombre no puede tener más de 100 caracteres.")]
        public string Nombre { get; set; }
        public byte Estado { get; set; }
    }
}
