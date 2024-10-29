using Microsoft.Data.SqlClient;
using System.Data;

namespace ICA.Models
{
    public class RepositorioJuego : RepositorioBase, IRepositorioJuego
    {
        public RepositorioJuego(IConfiguration configuration) : base(configuration)
        {
           
        }
        public int Alta(Juego entidad)
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
                                INSERT INTO Juego (Titulo, Desarrollador, Fecha, Descripcion, Plataforma, Imagen, Video, Link, Estado, GeneroId, EtiquetaId, MateriaId)
                                VALUES (@titulo, @desarrollador, @fecha, @descripcion, @plataforma, @imagen, @video, @link, @estado, @generoId, @etiquetaId, @materiaId);
                                SELECT SCOPE_IDENTITY();";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@titulo", entidad.Titulo);
                command.Parameters.AddWithValue("@desarrollador", entidad.Desarrollador);
                command.Parameters.AddWithValue("@fecha", entidad.Fecha ?? (object)DBNull.Value); // Manejo de null
                command.Parameters.AddWithValue("@descripcion", entidad.Descripcion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@plataforma", entidad.Plataforma ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@imagen", entidad.Imagen ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@video", entidad.Video ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@link", entidad.Link ?? (object)DBNull.Value);
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
                    string sql = @"DELETE FROM Juego WHERE Id = @id";

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

        public int Modificacion(Juego entidad)
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
                    UPDATE Juego
                    SET Titulo = @titulo, 
                        Descripcion = @descripcion, 
                        Desarrollador = @desarrollador, 
                        Fecha = @fecha, 
                       
                        Video = @video, 
                        Imagen = @imagen, 
                        Link = @link,
                        Estado = @estado, 
                        GeneroId = @generoId, 
                        EtiquetaId = @etiquetaId, 
                        MateriaId = @materiaId
                    WHERE Id = @id";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        // Usar AddWithValue con cuidado: asegúrate de que el tipo de datos sea correcto
                        command.Parameters.AddWithValue("@titulo", entidad.Titulo);
                        command.Parameters.AddWithValue("@descripcion", entidad.Descripcion);
                        command.Parameters.AddWithValue("@desarrollador", entidad.Desarrollador);
                        command.Parameters.AddWithValue("@fecha", entidad.Fecha);
                        
                        command.Parameters.Add("@video", SqlDbType.NVarChar).Value = entidad.Video ?? (object)DBNull.Value;
                        command.Parameters.Add("@imagen", SqlDbType.NVarChar).Value = entidad.Imagen ?? (object)DBNull.Value;
                        command.Parameters.Add("@link", SqlDbType.NVarChar).Value = entidad.Link ?? (object)DBNull.Value;
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
                throw new ApplicationException("Se produjo un error al intentar modificar el proyecto.", ex);
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

        public IList<Juego> ObtenerTodos()
        {
            var juegos = new List<Juego>();

            string query = @"SELECT j.Id, j.Titulo, j.Desarrollador, j.Fecha, j.Descripcion, j.Imagen, j.Video, j.Link, j.Integrantes, j.Estado, j.GeneroId, j.EtiquetaId, j.MateriaId,
                     g.Nombre AS GeneroNombre, m.Nombre AS MateriaNombre, e.Nombre AS EtiquetaNombre
              FROM Juego j
              INNER JOIN Genero g ON g.Id = j.GeneroId
              INNER JOIN Materia m ON m.Id = j.MateriaId
              INNER JOIN Etiqueta e ON e.Id = j.EtiquetaId";

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
                            var juego = new Juego
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Titulo = reader.GetString(reader.GetOrdinal("Titulo")),
                                Desarrollador = reader.GetString(reader.GetOrdinal("Desarrollador")),
                                
                                Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
                                Imagen = reader.IsDBNull(reader.GetOrdinal("Imagen")) ? null : reader.GetString(reader.GetOrdinal("Imagen")),
                                Video = reader.IsDBNull(reader.GetOrdinal("Video")) ? null : reader.GetString(reader.GetOrdinal("Video")),
                                Link = reader.IsDBNull(reader.GetOrdinal("Link")) ? null : reader.GetString(reader.GetOrdinal("Link")),
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
                            juegos.Add(juego);
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

            return juegos;
        }

        public Juego ObtenerPorId(int id)
        {
            // Validación del parámetro id
            if (id <= 0)
            {
                throw new ArgumentException("El identificador debe ser un valor positivo.", nameof(id));
            }

            Juego juego = null;
            string query = @"SELECT j.Id, j.Titulo, j.Desarrollador, j.Fecha, j.Descripcion, j.Imagen, j.Video, j.Link, j.Integrantes, j.Estado, j.GeneroId, j.EtiquetaId, j.MateriaId,
                                    g.Nombre AS GeneroNombre, m.Nombre AS MateriaNombre, e.Nombre AS EtiquetaNombre
                     FROM Juego j 
                     INNER JOIN Genero g ON g.Id = j.GeneroId
                     INNER JOIN Materia m ON m.Id = j.MateriaId
                     INNER JOIN Etiqueta e ON e.Id = j.EtiquetaId
                     WHERE j.Id = @id";

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
                            juego = new Juego
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Titulo = reader.GetString(reader.GetOrdinal("Titulo")),
                                Desarrollador = reader.GetString(reader.GetOrdinal("Desarrollador")),
                                Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
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

            return juego;
        }
    }
}
