using Microsoft.Data.SqlClient;
using System.Data;

namespace ICA.Models
{
    public class RepositorioPublicidad : RepositorioBase, IRepositorioPublicidad
    {
        public RepositorioPublicidad(IConfiguration configuration) : base(configuration)
        {

        }
        public int Alta(Publicidad entidad)
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
                                INSERT INTO Publicidad (Titulo, Creador, Fecha, Descripcion, Integrantes, Imagen, Video, Estado, GeneroId, EtiquetaId, MateriaId)
                                VALUES (@titulo, @creador, @fecha, @descripcion, @integrantes, @imagen, @video, @estado, @generoId, @etiquetaId, @materiaId);
                                SELECT SCOPE_IDENTITY();";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@titulo", entidad.Titulo);
                command.Parameters.AddWithValue("@creador", entidad.Creador);
                command.Parameters.AddWithValue("@fecha", entidad.Fecha ?? (object)DBNull.Value); // Manejo de null
                command.Parameters.AddWithValue("@descripcion", entidad.Descripcion);
                command.Parameters.AddWithValue("@integrantes", entidad.Integrantes);
                command.Parameters.AddWithValue("@imagen", entidad.Imagen ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@video", entidad.Video ?? (object)DBNull.Value);
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
                    string sql = @"DELETE FROM Publicidad WHERE Id = @id";

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

        public int Modificacion(Publicidad entidad)
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
                    UPDATE Publicidad
                    SET Titulo = @titulo, 
                        Descripcion = @descripcion,
                        Integrantes = @integrantes,
                        Creador = @creador, 
                        Fecha = @fecha, 
                        Video = @video,
                        Imagen = @imagen,
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
                        command.Parameters.AddWithValue("@integrantes", entidad.Integrantes);
                        command.Parameters.AddWithValue("@creador", entidad.Creador);
                        command.Parameters.AddWithValue("@fecha", entidad.Fecha);
                        command.Parameters.AddWithValue("@video", string.IsNullOrEmpty(entidad.Video) ? (object)DBNull.Value : entidad.Video);
                        command.Parameters.AddWithValue("@imagen", string.IsNullOrEmpty(entidad.Imagen) ? (object)DBNull.Value : entidad.Imagen);
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
                throw new ApplicationException("Se produjo un error al intentar modificar el publicidad.", ex);
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

        public IList<Publicidad> ObtenerTodos()
        {
            var peliculas = new List<Publicidad>();

            string query = @"SELECT c.Id, c.Titulo, c.Creador, c.Fecha, c.Descripcion,  c.Imagen,  c.Video, c.Integrantes, c.Estado, c.GeneroId, c.EtiquetaId, c.MateriaId,
                     g.Nombre AS GeneroNombre, m.Nombre AS MateriaNombre, e.Nombre AS EtiquetaNombre
              FROM Publicidad c
              INNER JOIN Genero g ON g.Id = c.GeneroId
              INNER JOIN Materia m ON m.Id = c.MateriaId
              INNER JOIN Etiqueta e ON e.Id = c.EtiquetaId";

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
                            var publicidad = new Publicidad
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Titulo = reader.GetString(reader.GetOrdinal("Titulo")),
                                Creador = reader.GetString(reader.GetOrdinal("Creador")),
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
                            peliculas.Add(publicidad);
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

        public Publicidad ObtenerPorId(int id)
        {
            // Validación del parámetro id
            if (id <= 0)
            {
                throw new ArgumentException("El identificador debe ser un valor positivo.", nameof(id));
            }

            Publicidad comunicacion = null;
            string query = @"SELECT c.Id, c.Titulo, c.Descripcion, c.Integrantes, c.Creador, c.Fecha, c.Estado, c.Video, c.Imagen, c.GeneroId,c.EtiquetaId, c.MateriaId,
                            g.Nombre AS GeneroNombre, m.Nombre AS MateriaNombre, e.Nombre AS EtiquetaNombre
                     FROM Publicidad c 
                     INNER JOIN Genero g ON g.Id = c.GeneroId
                     INNER JOIN Materia m ON m.Id = c.MateriaId
                     INNER JOIN Etiqueta e ON e.Id = c.EtiquetaId
                     WHERE c.Id = @id";

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
                            comunicacion = new Publicidad
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Titulo = reader.GetString(reader.GetOrdinal("Titulo")),
                                Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
                                Integrantes = reader.IsDBNull(reader.GetOrdinal("Integrantes")) ? null : reader.GetString(reader.GetOrdinal("Integrantes")),
                                Creador = reader.GetString(reader.GetOrdinal("Creador")),
                                Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                                Video = reader.IsDBNull(reader.GetOrdinal("Video")) ? null : reader.GetString(reader.GetOrdinal("Video")),
                                Imagen = reader.IsDBNull(reader.GetOrdinal("Imagen")) ? null : reader.GetString(reader.GetOrdinal("Imagen")),
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

            return comunicacion;
        }
    }
}