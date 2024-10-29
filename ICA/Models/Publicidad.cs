using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ICA.Models
{
    public class Publicidad
    {
        public int Id { get; set; } // Corresponde a la columna ID

        [Required(ErrorMessage = "El campo Título es obligatorio.")]
        [StringLength(100, ErrorMessage = "El campo Título no puede tener más de 100 caracteres.")]
        public string Titulo { get; set; } // Corresponde a la columna Título

        [Required(ErrorMessage = "El campo Creador es obligatorio.")]
        [StringLength(100, ErrorMessage = "El campo Creador no puede tener más de 100 caracteres.")]
        [Display(Name = "Realizador")]
        public string Creador { get; set; } // Corresponde a la columna Creador

        [StringLength(1000, ErrorMessage = "Los Integrantes no puede tener más de 500 caracteres.")]
        public string? Integrantes { get; set; } // Corresponde a la columna Integrantes

        [Required(ErrorMessage = "El campo Fecha es obligatorio.")]
        [DataType(DataType.Date, ErrorMessage = "La fecha debe ser una fecha válida.")]
        public DateTime? Fecha { get; set; } // Corresponde a la columna Fecha

        [StringLength(1000, ErrorMessage = "La descripción no puede tener más de 1000 caracteres.")]
        public string? Descripcion { get; set; } // Corresponde a la columna Sinopsis

        [Url(ErrorMessage = "El campo Imagen debe ser una URL válida.")]
        public string? Imagen { get; set; } // Corresponde a la columna Imagen

        [Url(ErrorMessage = "El campo Video debe ser una URL válida.")]
        public string? Video { get; set; } // Corresponde a la columna Video

        [Required(ErrorMessage = "El campo Estado es obligatorio.")]
        [Range(0, 255, ErrorMessage = "El campo Estado debe estar entre 0 y 255.")]
        public byte Estado { get; set; } // Corresponde a la columna Estado

        public int? GeneroId { get; set; }
        public Genero? Genero { get; set; }
        public int? MateriaId { get; set; }
        public Materia? Materia { get; set; }
        public int? EtiquetaId { get; set; }
        public Etiqueta? Etiqueta { get; set; }

        [NotMapped]
        public IFormFile? ImagenFile { get; set; } // No mapeado, no validado directamente

        [NotMapped]
        public string? VideoId
        {
            get
            {
                if (Uri.TryCreate(Video, UriKind.Absolute, out var uri))
                {
                    // Manejar URLs con parámetros de consulta, como https://www.youtube.com/watch?v=XYZ123
                    var query = HttpUtility.ParseQueryString(uri.Query);
                    var videoIdFromQuery = query["v"];
                    if (!string.IsNullOrEmpty(videoIdFromQuery))
                    {
                        return videoIdFromQuery;
                    }

                    // Manejar URLs acortadas y embed, como https://youtu.be/XYZ123 o https://www.youtube.com/embed/XYZ123
                    var videoIdMatch = System.Text.RegularExpressions.Regex.Match(uri.AbsolutePath, @"(?:\/embed\/|\/v\/|\/)([a-zA-Z0-9_-]{11})");
                    if (videoIdMatch.Success)
                    {
                        return videoIdMatch.Groups[1].Value;
                    }
                }

                return null;

            }
        }
    }
}

