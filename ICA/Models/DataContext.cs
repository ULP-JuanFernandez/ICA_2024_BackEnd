using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ICA.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<Proyecto> Proyectos { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Tecnicatura> Tecnicaturas { get; set; }
        public DbSet<Slide> Sliders { get; set; }
        public DbSet<Etiqueta> Etiquetas { get; set; }

    }
}
