using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ICA.Models
{
    public class Proyecto
    {
        [Key]
        [Display(Name = "Código Int.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El campo Nombre no puede tener más de 100 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo Creador es obligatorio.")]
        [StringLength(100, ErrorMessage = "El campo Creador no puede tener más de 100 caracteres.")]
        public string Creador { get; set; }

        [Required(ErrorMessage = "El campo Fecha es obligatorio.")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El campo Descripción es obligatorio.")]
        [StringLength(500, ErrorMessage = "El campo Descripción no puede tener más de 500 caracteres.")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El campo Género es obligatorio.")]
        public int GeneroId { get; set; }
        public Genero? Genero { get; set; }

        
        public int TecnicaturaId { get; set; }
        public Tecnicatura? Tecnicatura { get; set; }

        public int EtiquetaId { get; set; }
        public Etiqueta? Etiqueta { get; set; }

        [Required(ErrorMessage = "El campo Link es obligatorio.")]
        [Url(ErrorMessage = "El campo Link debe ser una URL válida.")]
        public string Link { get; set; }

        public string? VideoId
        {
            get
            {
                if (Uri.TryCreate(Link, UriKind.Absolute, out var uri))
                {
                    var query = HttpUtility.ParseQueryString(uri.Query);
                    return query["v"];
                }

                return null; // Retorna null si Link no es una URL válida.
            }
        }
    }
}