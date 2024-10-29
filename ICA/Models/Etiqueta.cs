using ICA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace ICA.Models
{
    public class Etiqueta
    {
        [Key]
        [Display(Name = "Código")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El campo Nombre no puede tener más de 50 caracteres.")]
        public string Nombre { get; set; }
        
        [StringLength(500, ErrorMessage = "El campo Descripción no puede tener más de 500 caracteres.")]
        public string? Descripcion { get; set; }
        public byte Estado { get; set; }

        [Required(ErrorMessage = "El campo Tecnicatura es obligatorio.")]
        public int? TecnicaturaId { get; set; }
        public Tecnicatura? Tecnicatura { get; set; }

        
        
    }
}




