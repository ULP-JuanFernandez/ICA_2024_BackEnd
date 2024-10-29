using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ICA.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<Tecnicatura> Tecnicaturas { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Etiqueta> Etiquetas { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<Juego> Juegos { get; set; }
        public DbSet<Publicidad> Publicidades { get; set; }
        public DbSet<Comunicacion> Comunicaciones { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }




        public DbSet<Proyecto> Proyectos { get; set; }
       

        
        
        

        public DbSet<Slide> Sliders { get; set; }
    }
}
