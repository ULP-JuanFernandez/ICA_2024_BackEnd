using Microsoft.Data.SqlClient;
using System.Data;

namespace ICA.Models
{
    public class RepositorioMateria : RepositorioBase, IRepositorioMateria
    {
        public RepositorioMateria(IConfiguration configuration) : base(configuration)
        {

        }
        public int Alta(Materia entidad)
        {
            if (entidad == null)
            {
                throw new ArgumentNullException(nameof(entidad), "La entidad no puede ser nula.");
            }

            if (string.IsNullOrWhiteSpace(entidad.Nombre))
            {
                throw new ArgumentException("El campo Nombre no puede estar vacío o ser nulo.", nameof(entidad.Nombre));
            }


            const string sql = @"
            INSERT INTO Materia (Nombre, Estado,TecnicaturaId)
            VALUES (@nombre, @estado, @tecnicaturaId);
            SELECT SCOPE_IDENTITY();";
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@nombre", entidad.Nombre);
                command.Parameters.AddWithValue("@estado", entidad.Estado);
                command.Parameters.AddWithValue("@tecnicaturaId", entidad.TecnicaturaId);

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

        public bool ExisteReferencias(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                string sql = @"SELECT COUNT(*) FROM Pelicula WHERE MateriaId = @id";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0; // Devuelve true si hay referencias
                }
            }
        }
        public int Baja(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("El identificador debe ser un valor positivo.", nameof(id));
            }
            if (ExisteReferencias(id))
            {
                throw new InvalidOperationException("No se puede eliminar la materia porque está en uso.");
            }
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"DELETE FROM Materia WHERE Id = @id";

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
                throw new ApplicationException("Se produjo un error al intentar eliminar la materia.", ex);
            }
        }

        public int Modificacion(Materia entidad)
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
            if (string.IsNullOrWhiteSpace(entidad.Nombre))
            {
                throw new ArgumentException("El campo Nombre no puede estar vacío o ser nulo.", nameof(entidad.Nombre));
            }
            int rowsAffected = 0;

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    const string sql = @"
                    UPDATE Materia
                    SET Nombre = @nombre, Estado = @estado, TecnicaturaId = @tecnicaturaId
                    WHERE Id = @id";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        // Usar AddWithValue con cuidado: asegúrate de que el tipo de datos sea correcto
                        command.Parameters.AddWithValue("@nombre", entidad.Nombre);
                        command.Parameters.AddWithValue("@estado", entidad.Estado);
                        command.Parameters.AddWithValue("@tecnicaturaId", entidad.TecnicaturaId);
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

        public IList<Materia> ObtenerTodos()
        {
            var materias = new List<Materia>();

            string query = @"SELECT m.Id, m.Nombre, m.Estado, m.TecnicaturaId, t.Nombre as TecnicaturaNombre
              FROM Materia m
              INNER JOIN Tecnicatura t ON t.Id = m.TecnicaturaId";

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
                            var materia = new Materia
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                Estado = reader.GetByte(reader.GetOrdinal("estado")),
                                TecnicaturaId = reader.GetInt32(reader.GetOrdinal("TecnicaturaId")),
                                Tecnicatura = new Tecnicatura
                                {
                                    Nombre = reader.IsDBNull(reader.GetOrdinal("TecnicaturaNombre")) ? null : reader.GetString(reader.GetOrdinal("TecnicaturaNombre"))
                                }
                                
                            };
                            materias.Add(materia);
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

            return materias;
        }
        public IList<Materia> ObtenerTodos(int id)
        {
            var materias = new List<Materia>();

            string query = @"SELECT m.Id, m.Nombre, m.Estado, m.TecnicaturaId, t.Nombre as TecnicaturaNombre
              FROM Materia m
              INNER JOIN Tecnicatura t ON t.Id = m.TecnicaturaId
              WHERE TecnicaturaId = @id";

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
                        while (reader.Read())
                        {
                            var materia = new Materia
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                Estado = reader.GetByte(reader.GetOrdinal("estado")),
                                TecnicaturaId = reader.GetInt32(reader.GetOrdinal("TecnicaturaId")),
                                Tecnicatura = new Tecnicatura
                                {
                                    Nombre = reader.IsDBNull(reader.GetOrdinal("TecnicaturaNombre")) ? null : reader.GetString(reader.GetOrdinal("TecnicaturaNombre"))
                                }

                            };
                            materias.Add(materia);
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

            return materias;
        }
        public Materia ObtenerPorId(int id)
        {
            // Validación del parámetro id
            if (id <= 0)
            {
                throw new ArgumentException("El identificador debe ser un valor positivo.", nameof(id));
            }

            Materia materia = null;
            string query = @"SELECT m.Id, m.Nombre, m.Estado, m.TecnicaturaId,
                             t.Nombre AS TecnicaturaNombre
                     FROM Materia m
                     INNER JOIN Tecnicatura t ON t.Id = m.TecnicaturaId
                     WHERE m.Id = @id";

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
                            materia = new Materia
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                Estado = reader.GetByte(reader.GetOrdinal("Estado")), 
                                TecnicaturaId = reader.GetInt32(reader.GetOrdinal("TecnicaturaId")),
                                Tecnicatura = new Tecnicatura
                                {
                                    Nombre = reader.GetString(reader.GetOrdinal("TecnicaturaNombre"))
                                },
                               
                            };
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Error relacionado con la base de datos
                throw new ApplicationException("Error al acceder a la base de datos para obtener la materia.", ex);
            }
            catch (Exception ex)
            {
                // Error general
                throw new ApplicationException("Se produjo un error inesperado al obtener la materia.", ex);
            }

            return materia;
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