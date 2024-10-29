using Humanizer;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ICA.Models
{
    public class RepositorioEtiqueta : RepositorioBase, IRepositorioEtiqueta
    {
        public RepositorioEtiqueta(IConfiguration configuration) : base(configuration)
        {

        }
        public int Alta(Etiqueta entidad)
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
            INSERT INTO Etiqueta (Nombre, Descripcion, Estado, TecnicaturaId)
            VALUES (@nombre, @descripcion, @estado, @tecnicaturaId);
            SELECT SCOPE_IDENTITY();";

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@nombre", entidad.Nombre);
                command.Parameters.AddWithValue("@descripcion", entidad.Descripcion ?? (object)DBNull.Value); 
                command.Parameters.AddWithValue("@estado", entidad.Estado);
                command.Parameters.AddWithValue("@tecnicaturaId", entidad.TecnicaturaId ?? (object)DBNull.Value);
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
                string sql = @"SELECT COUNT(*) FROM Pelicula WHERE EtiquetaId = @id";

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
                throw new InvalidOperationException("No se puede eliminar la etiqueta porque está en uso.");
            }
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"DELETE FROM Etiqueta WHERE Id = @id";

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
                throw new ApplicationException("Se produjo un error al intentar eliminar la etiqueta.", ex);
            }
        }

        public int Modificacion(Etiqueta entidad)
        {
            // Verificar si la entidad es nula
            if (entidad == null)
            {
                throw new ArgumentNullException(nameof(entidad), "La entidad no puede ser nula.");
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
                    const string sql = "UPDATE Etiqueta SET " +
                                       "Nombre = @nombre, Descripcion = @descripcion, Estado = @estado, TecnicaturaId = @tecnicaturaId " +
                                       "WHERE Id = @id";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        // Usar AddWithValue con cuidado: asegúrate de que el tipo de datos sea correcto
                        command.Parameters.AddWithValue("@nombre", entidad.Nombre);
                        command.Parameters.AddWithValue("@descripcion", entidad.Descripcion ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@estado", entidad.Estado);
                        command.Parameters.AddWithValue("@tecnicaturaId", entidad.TecnicaturaId ?? (object)DBNull.Value);
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
                throw new ApplicationException("Se produjo un error al intentar modificar el etiqueta.", ex);
            }
            catch (Exception ex)
            {
                // Manejo de errores generales
                // Ejemplo de registro de error:
                // Logger.LogError(ex, "Error inesperado: {Message}", ex.Message);
                throw new ApplicationException("Se produjo un error inesperado.", ex);
            }

            return rowsAffected;
        }

        public IList<Etiqueta> ObtenerTodos()
        {
            var etiquetas = new List<Etiqueta>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    const string sql = @"SELECT e.Id, e.Nombre, e.Descripcion, e.Estado, e.TecnicaturaId, t.Nombre as TecnicaturaNombre
                                 FROM Etiqueta e
                                 LEFT JOIN Tecnicatura t ON t.Id = e.TecnicaturaId"; // Usa LEFT JOIN para manejar nulos en Tecnicatura


                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;

                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var etiqueta = new Etiqueta
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                    Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion"))
                                                  ? null
                                                  : reader.GetString(reader.GetOrdinal("Descripcion")),
                                    Estado = reader.GetByte(reader.GetOrdinal("Estado")),
                                    TecnicaturaId = reader.IsDBNull(reader.GetOrdinal("TecnicaturaId"))
                                                  ? (int?)null
                                                  : reader.GetInt32(reader.GetOrdinal("TecnicaturaId")),
                                    Tecnicatura = new Tecnicatura
                                    {
                                        Nombre = reader.GetString(reader.GetOrdinal("TecnicaturaNombre"))
                                    }
                                };
                                etiquetas.Add(etiqueta);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Maneja errores específicos de SQL aquí
                // Log del error y/o manejar según la necesidad
                // Por ejemplo: Logger.LogError(ex, "Error al obtener los géneros.");
                throw new ApplicationException("Se produjo un error al intentar obtener las etiquetas.", ex);
            }
            catch (Exception ex)
            {
                // Maneja errores generales aquí
                // Log del error y/o manejar según la necesidad
                // Por ejemplo: Logger.LogError(ex, "Error inesperado.");
                throw new ApplicationException("Se produjo un error inesperado.", ex);
            }

            return etiquetas;
        }
        public IList<Etiqueta> ObtenerTodos(int id)
        {
            var etiquetas = new List<Etiqueta>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    const string sql = @"SELECT e.Id, e.Nombre, e.Descripcion, e.Estado, e.TecnicaturaId, t.Nombre as TecnicaturaNombre
                                 FROM Etiqueta e
                                 LEFT JOIN Tecnicatura t ON t.Id = e.TecnicaturaId
                                 WHERE TecnicaturaId = @id"; // Usa LEFT JOIN para manejar nulos en Tecnicatura


                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.CommandType = CommandType.Text;

                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var etiqueta = new Etiqueta
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                    Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion"))
                                                  ? null
                                                  : reader.GetString(reader.GetOrdinal("Descripcion")),
                                    Estado = reader.GetByte(reader.GetOrdinal("Estado")),
                                    TecnicaturaId = reader.IsDBNull(reader.GetOrdinal("TecnicaturaId"))
                                                  ? (int?)null
                                                  : reader.GetInt32(reader.GetOrdinal("TecnicaturaId")),
                                    Tecnicatura = new Tecnicatura
                                    {
                                        Nombre = reader.GetString(reader.GetOrdinal("Nombre"))
                                    }
                                };
                                etiquetas.Add(etiqueta);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Maneja errores específicos de SQL aquí
                // Log del error y/o manejar según la necesidad
                // Por ejemplo: Logger.LogError(ex, "Error al obtener los géneros.");
                throw new ApplicationException("Se produjo un error al intentar obtener las etiquetas.", ex);
            }
            catch (Exception ex)
            {
                // Maneja errores generales aquí
                // Log del error y/o manejar según la necesidad
                // Por ejemplo: Logger.LogError(ex, "Error inesperado.");
                throw new ApplicationException("Se produjo un error inesperado.", ex);
            }

            return etiquetas;
        }


        public Etiqueta ObtenerPorId(int id)
        {
            Etiqueta entidad = null;
            using (var connection = new SqlConnection(connectionString))
            {
                string sql = @"SELECT e.Id, e.Nombre, e.Descripcion, e.Estado, e.TecnicaturaId, t.Nombre as TecnicaturaNombre
                               FROM Etiqueta e
                               LEFT JOIN Tecnicatura t ON t.Id = e.TecnicaturaId
                               WHERE e.Id = @id";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        entidad = new Etiqueta
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                            Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion"))
                                                  ? null
                                                  : reader.GetString(reader.GetOrdinal("Descripcion")),
                            Estado = reader.GetByte(reader.GetOrdinal("Estado")),
                            TecnicaturaId = reader.IsDBNull(reader.GetOrdinal("TecnicaturaId"))
                                                  ? (int?)null
                                                  : reader.GetInt32(reader.GetOrdinal("TecnicaturaId")),
                            Tecnicatura = new Tecnicatura
                            {
                                Nombre = reader.GetString(reader.GetOrdinal("Nombre"))
                            }
                        };
                    }
                    connection.Close();
                }
            }
            return entidad;
        }

    }
}