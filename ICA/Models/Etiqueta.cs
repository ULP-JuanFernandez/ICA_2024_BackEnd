using System.ComponentModel.DataAnnotations;

namespace ICA.Models
{
    public class Etiqueta
    {
        [Key]
        [Display(Name = "Código Int.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El campo Nombre no puede tener más de 100 caracteres.")]
        public string Nombre { get; set; }


        [StringLength(500, ErrorMessage = "El campo Descripción no puede tener más de 500 caracteres.")]
        public string? Descripcion { get; set; }
    }
}
