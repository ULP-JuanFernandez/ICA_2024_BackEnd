using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ICA.Models
{
    public class Slide
    {
        [Key] // Marca esta propiedad como la clave primaria
        public int Id { get; set; }

        [StringLength(50)] // Limita el tamaño máximo del nombre a 50 caracteres
        public string Nombre { get; set; }

        [StringLength(500)] // Limita el tamaño máximo de la descripción a 500 caracteres
        public string Descripcion { get; set; }

        // Hace que esta propiedad sea obligatoria
        public DateTime? FechaCreacion { get; set; }

        public DateTime? FechaUltimaModificacion { get; set; } // Nullable para permitir valores nulos

        [StringLength(255)] // Limita el tamaño máximo del URL a 255 caracteres
        public string? Imagen { get; set; }

        public int? Orden { get; set; } // Nullable para permitir valores nulos

        [Required] // Hace que esta propiedad sea obligatoria
        public byte Estado { get; set; }
        public string EstadoDescription
        {
            get { return Estado == 1 ? "Habilitado" : "Deshabilitado"; }
        }
        public DateTime? FechaEliminacion { get; set; } // Nullable para permitir valores nulos

        // Propiedad no mapeada que se utiliza para manejar la carga de archivos
        [NotMapped]
        public IFormFile? ImagenFile { get; set; }
    }
}