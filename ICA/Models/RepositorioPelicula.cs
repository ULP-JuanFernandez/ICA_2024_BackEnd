﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using static System.Net.Mime.MediaTypeNames;

namespace ICA.Models
{
    public class RepositorioPelicula : RepositorioBase, IRepositorioPelicula
    {
        public RepositorioPelicula(IConfiguration configuration) : base(configuration)
        {

        }
        public int Alta(Pelicula entidad)
        {
            if (entidad == null)
            {
                throw new ArgumentNullException(nameof(entidad), "La entidad no puede ser nula.");
            }

            if (string.IsNullOrWhiteSpace(entidad.Titulo))
            {
                throw new ArgumentException("El campo Titulo no puede estar vacío o ser nulo.", nameof(entidad.Titulo));
            }

            const string sql = @"
                                INSERT INTO Pelicula (Titulo, Creador, Fecha, Sinopsis, Integrantes, Imagen, Video, Estado, GeneroId, EtiquetaId, MateriaId)
                                VALUES (@titulo, @creador, @fecha, @sinopsis, @integrantes, @imagen, @video, @estado, @generoId, @etiquetaId, @materiaId);
                                SELECT SCOPE_IDENTITY();";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@titulo", entidad.Titulo);
                command.Parameters.AddWithValue("@creador", entidad.Creador);
                command.Parameters.AddWithValue("@fecha", entidad.Fecha ?? (object)DBNull.Value); // Manejo de null
                command.Parameters.AddWithValue("@sinopsis", entidad.Sinopsis);
                command.Parameters.AddWithValue("@integrantes", entidad.Integrantes);
                command.Parameters.AddWithValue("@imagen", entidad.Imagen?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@video", entidad.Video);
                command.Parameters.AddWithValue("@estado", entidad.Estado);
                command.Parameters.AddWithValue("@generoId", entidad.GeneroId);
                command.Parameters.AddWithValue("@etiquetaId", entidad.EtiquetaId);
                command.Parameters.AddWithValue("@materiaId", entidad.MateriaId);

                connection.Open();

                var result = command.ExecuteScalar();
                if (result != null && int.TryParse(result.ToString(), out var newId))
                {
                    entidad.Id = newId;
                    return newId;
                }
                else
                {
                    throw new InvalidOperationException("No se pudo obtener el ID del nuevo registro.");
                }
            }
        }


        public int Baja(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El identificador debe ser un valor positivo.", nameof(id));
            }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"DELETE FROM Pelicula WHERE Id = @id";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@id", id);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        return rowsAffected;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new ApplicationException("Se produjo un error al intentar eliminar el proyecto.", ex);
            }
        }

        public int Modificacion(Pelicula entidad)
        {
            if (entidad == null)
            {
                throw new ArgumentNullException(nameof(entidad), "La entidad no puede ser nula.");
            }

            if (entidad.Id <= 0)
            {
                throw new ArgumentException("El identificador de la entidad debe ser un valor positivo.", nameof(entidad.Id));
            }
            // Verificar si los campos necesarios están presentes
            if (string.IsNullOrWhiteSpace(entidad.Titulo))
            {
                throw new ArgumentException("El campo Nombre no puede estar vacío o ser nulo.", nameof(entidad.Titulo));
            }
            int rowsAffected = 0;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    const string sql = @"
                    UPDATE Pelicula
                    SET Titulo = @titulo, 
                        Sinopsis = @sinopsis,
                        Integrantes = @integrantes,
                        Creador = @creador, 
                        Fecha = @fecha, 
                        Video = @video, 
                        Estado = @estado, 
                        GeneroId = @generoId, 
                        EtiquetaId = @etiquetaId, 
                        MateriaId = @materiaId
                    WHERE Id = @id";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        // Usar AddWithValue con cuidado: asegúrate de que el tipo de datos sea correcto
                        command.Parameters.AddWithValue("@titulo", entidad.Titulo);
                        command.Parameters.AddWithValue("@sinopsis", entidad.Sinopsis);
                        command.Parameters.AddWithValue("@integrantes", entidad.Integrantes);
                        command.Parameters.AddWithValue("@creador", entidad.Creador);
                        command.Parameters.AddWithValue("@fecha", entidad.Fecha);
                        command.Parameters.AddWithValue("@video", entidad.Video);
                        command.Parameters.AddWithValue("@estado", entidad.Estado);
                        command.Parameters.AddWithValue("@generoId", entidad.GeneroId);
                        command.Parameters.AddWithValue("@etiquetaId", entidad.EtiquetaId);
                        command.Parameters.AddWithValue("@materiaId", entidad.MateriaId);
                        command.Parameters.AddWithValue("@id", entidad.Id);

                        command.CommandType = CommandType.Text;

                        connection.Open();

                        // Ejecutar la consulta y obtener el número de filas afectadas
                        rowsAffected = command.ExecuteNonQuery();
                    }


                }
            }
            catch (SqlException ex)
            {
                // Manejo de errores específicos de SQL
                // Ejemplo de registro de error:
                // Logger.LogError(ex, "Error al modificar el género: {Message}", ex.Message);
                throw new ApplicationException("Se produjo un error al intentar modificar el pelicula.", ex);
            }
            catch (Exception ex)
            {
                // Manejo de errores generales
                // Ejemplo de registro de error:
                // Logger.LogError(ex, "Error inesperado: {Message}", ex.Message);}
                throw new ApplicationException("Se produjo un error inesperado.", ex);
            }

            return rowsAffected;
        }

        public IList<Pelicula> ObtenerTodos()
        {
            var peliculas = new List<Pelicula>();

            string query = @"SELECT p.Id, p.Titulo, p.Creador, p.Fecha, p.Sinopsis,  p.Imagen,  p.Video, p.Integrantes, p.Estado, p.GeneroId, p.EtiquetaId, p.MateriaId,
                     g.Nombre AS GeneroNombre, m.Nombre AS MateriaNombre, e.Nombre AS EtiquetaNombre
              FROM Pelicula p
              INNER JOIN Genero g ON g.Id = p.GeneroId
              INNER JOIN Materia m ON m.Id = p.MateriaId
              INNER JOIN Etiqueta e ON e.Id = p.EtiquetaId";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var pelicula = new Pelicula
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Titulo = reader.GetString(reader.GetOrdinal("Titulo")),
                                Creador = reader.GetString(reader.GetOrdinal("Creador")),
                                Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                                Sinopsis = reader.IsDBNull(reader.GetOrdinal("Sinopsis")) ? null : reader.GetString(reader.GetOrdinal("Sinopsis")),
                                Imagen = reader.IsDBNull(reader.GetOrdinal("Imagen")) ? null : reader.GetString(reader.GetOrdinal("Imagen")),
                                Video = reader.IsDBNull(reader.GetOrdinal("Video")) ? null : reader.GetString(reader.GetOrdinal("Video")),
                                Integrantes = reader.IsDBNull(reader.GetOrdinal("Integrantes")) ? null : reader.GetString(reader.GetOrdinal("Integrantes")),
                                Estado = reader.GetByte(reader.GetOrdinal("Estado")),
                                GeneroId = reader.GetInt32(reader.GetOrdinal("GeneroId")),
                                EtiquetaId = reader.GetInt32(reader.GetOrdinal("EtiquetaId")),
                                MateriaId = reader.GetInt32(reader.GetOrdinal("MateriaId")),

                                Genero = new Genero
                                {
                                    Nombre = reader.IsDBNull(reader.GetOrdinal("GeneroNombre")) ? null : reader.GetString(reader.GetOrdinal("GeneroNombre"))
                                },
                                Materia = new Materia
                                {
                                    Nombre = reader.IsDBNull(reader.GetOrdinal("MateriaNombre")) ? null : reader.GetString(reader.GetOrdinal("MateriaNombre"))
                                },
                                Etiqueta = new Etiqueta
                                {
                                    Nombre = reader.IsDBNull(reader.GetOrdinal("EtiquetaNombre")) ? null : reader.GetString(reader.GetOrdinal("EtiquetaNombre"))
                                }
                            };
                            peliculas.Add(pelicula);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Log specific SQL errors here
                // For example: Logger.LogError(ex, "Error al obtener los proyectos.");
                throw new ApplicationException("Se produjo un error al intentar obtener los proyectos desde la base de datos.", ex);
            }
            catch (Exception ex)
            {
                // Log general errors here
                // For example: Logger.LogError(ex, "Error inesperado al obtener proyectos.");
                throw new ApplicationException("Se produjo un error inesperado al intentar obtener los proyectos.", ex);
            }

            return peliculas;
        }

        public Pelicula ObtenerPorId(int id)
        {
            // Validación del parámetro id
            if (id <= 0)
            {
                throw new ArgumentException("El identificador debe ser un valor positivo.", nameof(id));
            }

            Pelicula pelicula = null;
            string query = @"SELECT p.Id, p.Titulo, p.Sinopsis, p.Integrantes, p.Creador, p.Fecha, p.Estado, p.Video, p.GeneroId, p.EtiquetaId, p.MateriaId,
                            g.Nombre AS GeneroNombre, m.Nombre AS MateriaNombre, e.Nombre AS EtiquetaNombre
                     FROM Pelicula p 
                     INNER JOIN Genero g ON g.Id = p.GeneroId
                     INNER JOIN Materia m ON m.Id = p.MateriaId
                     INNER JOIN Etiqueta e ON e.Id = p.EtiquetaId
                     WHERE p.Id = @id";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.CommandType = CommandType.Text;

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pelicula = new Pelicula
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Titulo = reader.GetString(reader.GetOrdinal("Titulo")),
                                Sinopsis = reader.GetString(reader.GetOrdinal("Sinopsis")),
                                Integrantes = reader.IsDBNull(reader.GetOrdinal("Integrantes")) ? null : reader.GetString(reader.GetOrdinal("Integrantes")),
                                Creador = reader.GetString(reader.GetOrdinal("Creador")),
                                Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                                Video = reader.GetString(reader.GetOrdinal("Video")),
                                Estado = reader.GetByte(reader.GetOrdinal("Estado")),
                                GeneroId = reader.GetInt32(reader.GetOrdinal("GeneroId")),
                                EtiquetaId = reader.GetInt32(reader.GetOrdinal("EtiquetaId")),
                                MateriaId = reader.GetInt32(reader.GetOrdinal("MateriaId")),
                               

                                Genero = new Genero
                                {
                                    Nombre = reader.GetString(reader.GetOrdinal("GeneroNombre"))
                                },
                                Materia = new Materia
                                {
                                    Nombre = reader.GetString(reader.GetOrdinal("MateriaNombre"))
                                },
                                Etiqueta = new Etiqueta
                                {
                                    Nombre = reader.IsDBNull(reader.GetOrdinal("EtiquetaNombre")) ? null : reader.GetString(reader.GetOrdinal("EtiquetaNombre"))
                                }
                            };
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Error relacionado con la base de datos
                throw new ApplicationException("Error al acceder a la base de datos para obtener el proyecto.", ex);
            }
            catch (Exception ex)
            {
                // Error general
                throw new ApplicationException("Se produjo un error inesperado al obtener el proyecto.", ex);
            }

            return pelicula;
        }


        //public IList<Proyecto> ObtenerSoloTresOrdenaPorIdAudioVisual()
        //{
        //    var proyectos = new List<Proyecto>();

        //    string query = @"SELECT TOP 3 Id, Nombre, Descripcion, Creador, Fecha, GeneroId, TecnicaturaId, Link        
        //                     FROM Proyectos 
        //                     WHERE TecnicaturaId = 1
        //                     ORDER BY Id DESC";

        //    try
        //    {
        //        using (var connection = new SqlConnection(connectionString))
        //        using (var command = new SqlCommand(query, connection))
        //        {
        //            command.CommandType = CommandType.Text;

        //            connection.Open();

        //            using (var reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    var proyecto = new Proyecto
        //                    {
        //                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
        //                        Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
        //                        Creador = reader.GetString(reader.GetOrdinal("Creador")),
        //                        Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
        //                        GeneroId = reader.GetInt32(reader.GetOrdinal("GeneroId")),
        //                        TecnicaturaId = reader.GetInt32(reader.GetOrdinal("TecnicaturaId")),
        //                        Link = reader.IsDBNull(reader.GetOrdinal("Link")) ? null : reader.GetString(reader.GetOrdinal("Link")),


        //                    };
        //                    proyectos.Add(proyecto);
        //                }
        //            }
        //        }
        //    }
        //    catch (SqlException ex)
        //    {
        //        // Log specific SQL errors here
        //        // For example: Logger.LogError(ex, "Error al obtener los proyectos.");
        //        throw new ApplicationException("Se produjo un error al intentar obtener los proyectos desde la base de datos.", ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log general errors here
        //        // For example: Logger.LogError(ex, "Error inesperado al obtener proyectos.");
        //        throw new ApplicationException("Se produjo un error inesperado al intentar obtener los proyectos.", ex);
        //    }

        //    return proyectos;
        //}
        //public IList<Proyecto> ObtenerSoloTresOrdenaPorIdVideosJuegos()
        //{
        //    var proyectos = new List<Proyecto>();

        //    string query = @"SELECT TOP 3 Id, Nombre, Descripcion, Creador, Fecha, GeneroId, TecnicaturaId, Link        
        //                     FROM Proyectos 
        //                     WHERE TecnicaturaId = 2
        //                     ORDER BY Id DESC";

        //    try
        //    {
        //        using (var connection = new SqlConnection(connectionString))
        //        using (var command = new SqlCommand(query, connection))
        //        {
        //            command.CommandType = CommandType.Text;

        //            connection.Open();

        //            using (var reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    var proyecto = new Proyecto
        //                    {
        //                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
        //                        Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
        //                        Creador = reader.GetString(reader.GetOrdinal("Creador")),
        //                        Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
        //                        GeneroId = reader.GetInt32(reader.GetOrdinal("GeneroId")),
        //                        TecnicaturaId = reader.GetInt32(reader.GetOrdinal("TecnicaturaId")),
        //                        Link = reader.IsDBNull(reader.GetOrdinal("Link")) ? null : reader.GetString(reader.GetOrdinal("Link")),


        //                    };
        //                    proyectos.Add(proyecto);
        //                }
        //            }
        //        }
        //    }
        //    catch (SqlException ex)
        //    {
        //        // Log specific SQL errors here
        //        // For example: Logger.LogError(ex, "Error al obtener los proyectos.");
        //        throw new ApplicationException("Se produjo un error al intentar obtener los proyectos desde la base de datos.", ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log general errors here
        //        // For example: Logger.LogError(ex, "Error inesperado al obtener proyectos.");
        //        throw new ApplicationException("Se produjo un error inesperado al intentar obtener los proyectos.", ex);
        //    }

        //    return proyectos;
        //}

        //public async Task<IList<Proyecto>> ObtenerTodosAudioVisual(int pageNumber, int pageSize)
        //{
        //    var proyectos = new List<Proyecto>();
        //    string query = @"
        //    SELECT p.Id, p.Nombre, p.Descripcion, p.Creador, p.Fecha, p.GeneroId, p.TecnicaturaId, p.Link,
        //           g.Nombre AS GeneroNombre, t.Nombre AS TecnicaturaNombre
        //    FROM Proyectos p
        //    INNER JOIN Generos g ON g.Id = p.GeneroId
        //    INNER JOIN Tecnicaturas t ON t.Id = p.TecnicaturaId
        //    WHERE p.TecnicaturaId = 1
        //    ORDER BY p.Fecha DESC
        //    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        //    using (var connection = new SqlConnection(connectionString))
        //    using (var command = new SqlCommand(query, connection))
        //    {
        //        command.Parameters.AddWithValue("@Offset", (pageNumber - 1) * pageSize);
        //        command.Parameters.AddWithValue("@PageSize", pageSize);

        //        await connection.OpenAsync();

        //        using (var reader = await command.ExecuteReaderAsync())
        //        {
        //            while (await reader.ReadAsync())
        //            {
        //                var proyecto = new Proyecto
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
        //                    Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
        //                    Creador = reader.GetString(reader.GetOrdinal("Creador")),
        //                    Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
        //                    GeneroId = reader.GetInt32(reader.GetOrdinal("GeneroId")),
        //                    TecnicaturaId = reader.GetInt32(reader.GetOrdinal("TecnicaturaId")),
        //                    Link = reader.IsDBNull(reader.GetOrdinal("Link")) ? null : reader.GetString(reader.GetOrdinal("Link")),
        //                    Genero = new Genero { Nombre = reader.IsDBNull(reader.GetOrdinal("GeneroNombre")) ? null : reader.GetString(reader.GetOrdinal("GeneroNombre")) },
        //                    Tecnicatura = new Tecnicatura { Nombre = reader.IsDBNull(reader.GetOrdinal("TecnicaturaNombre")) ? null : reader.GetString(reader.GetOrdinal("TecnicaturaNombre")) }
        //                };
        //                proyectos.Add(proyecto);
        //            }
        //        }
        //    }
        //    return proyectos;
        //}

        //public async Task<int> ObtenerTotalProyectosAudioVisual()
        //{
        //    string query = "SELECT COUNT(*) FROM Proyectos WHERE TecnicaturaId = 1";

        //    using (var connection = new SqlConnection(connectionString))
        //    using (var command = new SqlCommand(query, connection))
        //    {
        //        await connection.OpenAsync();
        //        return (int)await command.ExecuteScalarAsync();
        //    }
        //}
        //public IList<Proyecto> ObtenerTodosVideosJuegos()
        //{
        //    var proyectos = new List<Proyecto>();

        //    string query = @"SELECT p.Id, p.Nombre, p.Descripcion, p.Creador, p.Fecha, p.GeneroId, p.TecnicaturaId, p.Link,
        //             g.Nombre AS GeneroNombre, t.Nombre AS TecnicaturaNombre
        //      FROM Proyectos p
        //      INNER JOIN Generos g ON g.Id = p.GeneroId
        //      INNER JOIN Tecnicaturas t ON t.Id = p.TecnicaturaId
        //      WHERE P.TecnicaturaId = 2";

        //    try
        //    {
        //        using (var connection = new SqlConnection(connectionString))
        //        using (var command = new SqlCommand(query, connection))
        //        {
        //            command.CommandType = CommandType.Text;

        //            connection.Open();

        //            using (var reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    var proyecto = new Proyecto
        //                    {
        //                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
        //                        Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
        //                        Creador = reader.GetString(reader.GetOrdinal("Creador")),
        //                        Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
        //                        GeneroId = reader.GetInt32(reader.GetOrdinal("GeneroId")),
        //                        TecnicaturaId = reader.GetInt32(reader.GetOrdinal("TecnicaturaId")),
        //                        Link = reader.IsDBNull(reader.GetOrdinal("Link")) ? null : reader.GetString(reader.GetOrdinal("Link")),

        //                        Genero = new Genero
        //                        {
        //                            Nombre = reader.IsDBNull(reader.GetOrdinal("GeneroNombre")) ? null : reader.GetString(reader.GetOrdinal("GeneroNombre"))
        //                        },
        //                        Tecnicatura = new Tecnicatura
        //                        {
        //                            Nombre = reader.IsDBNull(reader.GetOrdinal("TecnicaturaNombre")) ? null : reader.GetString(reader.GetOrdinal("TecnicaturaNombre"))
        //                        }
        //                    };
        //                    proyectos.Add(proyecto);
        //                }
        //            }
        //        }
        //    }
        //    catch (SqlException ex)
        //    {
        //        // Log specific SQL errors here
        //        // For example: Logger.LogError(ex, "Error al obtener los proyectos.");
        //        throw new ApplicationException("Se produjo un error al intentar obtener los proyectos desde la base de datos.", ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log general errors here
        //        // For example: Logger.LogError(ex, "Error inesperado al obtener proyectos.");
        //        throw new ApplicationException("Se produjo un error inesperado al intentar obtener los proyectos.", ex);
        //    }

        //    return proyectos;
        //}

        //public IList<Proyecto> ObtenerTodosComunicacionMedios()
        //{
        //    var proyectos = new List<Proyecto>();

        //    string query = @"SELECT p.Id, p.Nombre, p.Descripcion, p.Creador, p.Fecha, p.GeneroId, p.TecnicaturaId, p.Link,
        //             g.Nombre AS GeneroNombre, t.Nombre AS TecnicaturaNombre
        //      FROM Proyectos p
        //      INNER JOIN Generos g ON g.Id = p.GeneroId
        //      INNER JOIN Tecnicaturas t ON t.Id = p.TecnicaturaId
        //      WHERE P.TecnicaturaId = 3";

        //    try
        //    {
        //        using (var connection = new SqlConnection(connectionString))
        //        using (var command = new SqlCommand(query, connection))
        //        {
        //            command.CommandType = CommandType.Text;

        //            connection.Open();

        //            using (var reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    var proyecto = new Proyecto
        //                    {
        //                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
        //                        Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
        //                        Creador = reader.GetString(reader.GetOrdinal("Creador")),
        //                        Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
        //                        GeneroId = reader.GetInt32(reader.GetOrdinal("GeneroId")),
        //                        TecnicaturaId = reader.GetInt32(reader.GetOrdinal("TecnicaturaId")),
        //                        Link = reader.IsDBNull(reader.GetOrdinal("Link")) ? null : reader.GetString(reader.GetOrdinal("Link")),

        //                        Genero = new Genero
        //                        {
        //                            Nombre = reader.IsDBNull(reader.GetOrdinal("GeneroNombre")) ? null : reader.GetString(reader.GetOrdinal("GeneroNombre"))
        //                        },
        //                        Tecnicatura = new Tecnicatura
        //                        {
        //                            Nombre = reader.IsDBNull(reader.GetOrdinal("TecnicaturaNombre")) ? null : reader.GetString(reader.GetOrdinal("TecnicaturaNombre"))
        //                        }
        //                    };
        //                    proyectos.Add(proyecto);
        //                }
        //            }
        //        }
        //    }
        //    catch (SqlException ex)
        //    {
        //        // Log specific SQL errors here
        //        // For example: Logger.LogError(ex, "Error al obtener los proyectos.");
        //        throw new ApplicationException("Se produjo un error al intentar obtener los proyectos desde la base de datos.", ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log general errors here
        //        // For example: Logger.LogError(ex, "Error inesperado al obtener proyectos.");
        //        throw new ApplicationException("Se produjo un error inesperado al intentar obtener los proyectos.", ex);
        //    }

        //    return proyectos;
        //}
        //public IList<Proyecto> ObtenerTodosPublicidad()
        //{
        //    var proyectos = new List<Proyecto>();

        //    string query = @"SELECT p.Id, p.Nombre, p.Descripcion, p.Creador, p.Fecha, p.GeneroId, p.TecnicaturaId, p.Link,
        //             g.Nombre AS GeneroNombre, t.Nombre AS TecnicaturaNombre
        //      FROM Proyectos p
        //      INNER JOIN Generos g ON g.Id = p.GeneroId
        //      INNER JOIN Tecnicaturas t ON t.Id = p.TecnicaturaId
        //      WHERE P.TecnicaturaId = 4";

        //    try
        //    {
        //        using (var connection = new SqlConnection(connectionString))
        //        using (var command = new SqlCommand(query, connection))
        //        {
        //            command.CommandType = CommandType.Text;

        //            connection.Open();

        //            using (var reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    var proyecto = new Proyecto
        //                    {
        //                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
        //                        Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
        //                        Creador = reader.GetString(reader.GetOrdinal("Creador")),
        //                        Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
        //                        GeneroId = reader.GetInt32(reader.GetOrdinal("GeneroId")),
        //                        TecnicaturaId = reader.GetInt32(reader.GetOrdinal("TecnicaturaId")),
        //                        Link = reader.IsDBNull(reader.GetOrdinal("Link")) ? null : reader.GetString(reader.GetOrdinal("Link")),

        //                        Genero = new Genero
        //                        {
        //                            Nombre = reader.IsDBNull(reader.GetOrdinal("GeneroNombre")) ? null : reader.GetString(reader.GetOrdinal("GeneroNombre"))
        //                        },
        //                        Tecnicatura = new Tecnicatura
        //                        {
        //                            Nombre = reader.IsDBNull(reader.GetOrdinal("TecnicaturaNombre")) ? null : reader.GetString(reader.GetOrdinal("TecnicaturaNombre"))
        //                        }
        //                    };
        //                    proyectos.Add(proyecto);
        //                }
        //            }
        //        }
        //    }
        //    catch (SqlException ex)
        //    {
        //        // Log specific SQL errors here
        //        // For example: Logger.LogError(ex, "Error al obtener los proyectos.");
        //        throw new ApplicationException("Se produjo un error al intentar obtener los proyectos desde la base de datos.", ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log general errors here
        //        // For example: Logger.LogError(ex, "Error inesperado al obtener proyectos.");
        //        throw new ApplicationException("Se produjo un error inesperado al intentar obtener los proyectos.", ex);
        //    }

        //    return proyectos;
        //}




        //public async Task<IEnumerable<Proyecto>> ObtenerAudioVisualPaginado(int pageIndex, int pageSize)
        //{
        //    var proyectos = new List<Proyecto>();

        //    // Calcula el número de registros a omitir
        //    int skip = (pageIndex - 1) * pageSize;

        //    string query = @"
        //    SELECT p.Id, p.Nombre, p.Descripcion, p.Creador, p.Fecha, p.GeneroId, p.TecnicaturaId, p.Link,
        //           g.Nombre AS GeneroNombre, t.Nombre AS TecnicaturaNombre
        //    FROM Proyectos p
        //    INNER JOIN Generos g ON g.Id = p.GeneroId
        //    INNER JOIN Tecnicaturas t ON t.Id = p.TecnicaturaId
        //    WHERE p.TecnicaturaId = 1
        //    ORDER BY p.Fecha DESC
        //    OFFSET @Skip ROWS
        //    FETCH NEXT @PageSize ROWS ONLY;";

        //    try
        //    {
        //        using (var connection = new SqlConnection(connectionString))
        //        using (var command = new SqlCommand(query, connection))
        //        {
        //            command.CommandType = CommandType.Text;
        //            command.Parameters.AddWithValue("@Skip", skip);
        //            command.Parameters.AddWithValue("@PageSize", pageSize);

        //            connection.Open();

        //            using (var reader = await command.ExecuteReaderAsync())
        //            {
        //                while (await reader.ReadAsync())
        //                {
        //                    var proyecto = new Proyecto
        //                    {
        //                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
        //                        Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
        //                        Creador = reader.GetString(reader.GetOrdinal("Creador")),
        //                        Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
        //                        GeneroId = reader.GetInt32(reader.GetOrdinal("GeneroId")),
        //                        TecnicaturaId = reader.GetInt32(reader.GetOrdinal("TecnicaturaId")),
        //                        Link = reader.IsDBNull(reader.GetOrdinal("Link")) ? null : reader.GetString(reader.GetOrdinal("Link")),

        //                        Genero = new Genero
        //                        {
        //                            Nombre = reader.IsDBNull(reader.GetOrdinal("GeneroNombre")) ? null : reader.GetString(reader.GetOrdinal("GeneroNombre"))
        //                        },
        //                        Tecnicatura = new Tecnicatura
        //                        {
        //                            Nombre = reader.IsDBNull(reader.GetOrdinal("TecnicaturaNombre")) ? null : reader.GetString(reader.GetOrdinal("TecnicaturaNombre"))
        //                        }
        //                    };
        //                    proyectos.Add(proyecto);
        //                }
        //            }
        //        }
        //    }
        //    catch (SqlException ex)
        //    {
        //        // Log specific SQL errors here
        //        throw new ApplicationException("Error al obtener los proyectos.", ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log general errors here
        //        throw new ApplicationException("Error inesperado al obtener los proyectos.", ex);
        //    }

        //    return proyectos;
        //}

        //public async Task<int> ObtenerTotalProyectos()
        //{
        //    string query = @"
        //    SELECT COUNT(*) 
        //    FROM Proyectos
        //    WHERE TecnicaturaId = 1";

        //    try
        //    {
        //        using (var connection = new SqlConnection(connectionString))
        //        using (var command = new SqlCommand(query, connection))
        //        {
        //            command.CommandType = CommandType.Text;

        //            connection.Open();
        //            return (int)await command.ExecuteScalarAsync();
        //        }
        //    }
        //    catch (SqlException ex)
        //    {
        //        // Log specific SQL errors here
        //        throw new ApplicationException("Error al obtener el total de proyectos.", ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log general errors here
        //        throw new ApplicationException("Error inesperado al obtener el total de proyectos.", ex);
        //    }
        //}

        //public IList<Proyecto> ObtenerFiltroPorTecnicaturaId(int id)
        //{
        //    var proyectos = new List<Proyecto>();

        //    string query = @"SELECT p.Id, p.Nombre, p.Descripcion, p.Creador, p.Fecha, p.GeneroId, p.TecnicaturaId, p.Link,
        //             g.Nombre AS GeneroNombre, t.Nombre AS TecnicaturaNombre
        //             FROM Proyectos p
        //             INNER JOIN Generos g ON g.Id = p.GeneroId
        //             INNER JOIN Tecnicaturas t ON t.Id = p.TecnicaturaId
        //             WHERE p.TecnicaturaId = @id";

        //    try
        //    {
        //        using (var connection = new SqlConnection(connectionString))
        //        using (var command = new SqlCommand(query, connection))
        //        {
        //            command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        //            command.CommandType = CommandType.Text;

        //            connection.Open();

        //            using (var reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    var proyecto = new Proyecto
        //                    {
        //                        Id = reader.GetFieldValue<int>(reader.GetOrdinal("Id")),
        //                        Nombre = reader.GetFieldValue<string>(reader.GetOrdinal("Nombre")),
        //                        Descripcion = reader.GetFieldValue<string>(reader.GetOrdinal("Descripcion")),
        //                        Creador = reader.GetFieldValue<string>(reader.GetOrdinal("Creador")),
        //                        Fecha = reader.GetFieldValue<DateTime>(reader.GetOrdinal("Fecha")),
        //                        GeneroId = reader.GetFieldValue<int>(reader.GetOrdinal("GeneroId")),
        //                        TecnicaturaId = reader.GetFieldValue<int>(reader.GetOrdinal("TecnicaturaId")),
        //                        Link = reader.GetFieldValue<string>(reader.GetOrdinal("Link")),

        //                        Genero = new Genero
        //                        {
        //                            Nombre = reader.GetFieldValue<string>(reader.GetOrdinal("GeneroNombre"))
        //                        },
        //                        Tecnicatura = new Tecnicatura
        //                        {
        //                            Nombre = reader.GetFieldValue<string>(reader.GetOrdinal("TecnicaturaNombre"))
        //                        }
        //                    };
        //                    proyectos.Add(proyecto);
        //                }
        //            }
        //        }
        //    }
        //    catch (SqlException ex)
        //    {
        //        throw new ApplicationException("Error al acceder a la base de datos para obtener los proyectos.", ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApplicationException("Se produjo un error inesperado al obtener los proyectos.", ex);
        //    }

        //    return proyectos;
        //}
    }
}