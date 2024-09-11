using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ICA.Models
{
    public class RepositorioSliders : RepositorioBase, IRepositorioSliders
    {
        public RepositorioSliders(IConfiguration configuration) : base(configuration)
        {

        }
        public int Alta(Slide entidad)
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
                        INSERT INTO Slide (Nombre, Descripcion, FechaCreacion,Imagen, Orden, Estado)
                        VALUES (@nombre, @descripcion, @fechaCreacion,@imagen, @orden, @estado);
                        SELECT SCOPE_IDENTITY();";

            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add(new SqlParameter("@nombre", SqlDbType.VarChar, 50) { Value = entidad.Nombre });
                    command.Parameters.Add(new SqlParameter("@descripcion", SqlDbType.VarChar, 500) { Value = entidad.Descripcion ?? (object)DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@fechaCreacion", SqlDbType.DateTime) { Value = DateTime.Now });
                    command.Parameters.Add(new SqlParameter("@imagen", SqlDbType.VarChar, 500) { Value = entidad.Imagen ?? (object)DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@orden", SqlDbType.Int) { Value = entidad.Orden });
                    command.Parameters.Add(new SqlParameter("@estado", SqlDbType.TinyInt) { Value = entidad.Estado });

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
            catch (SqlException ex)
            {
                // Manejo específico para errores de SQL
                throw new ApplicationException("Ocurrió un error al intentar guardar el slide.", ex);
            }
            catch (Exception ex)
            {
                // Manejo general de excepciones
                throw new ApplicationException("Ocurrió un error inesperado.", ex);
            }
        }
        public int Baja(int id)
        {
            // Validar el id para asegurarse de que es válido antes de continuar
            if (id <= 0)
            {
                throw new ArgumentException("El identificador debe ser un valor positivo.", nameof(id));
            }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"DELETE FROM Slide WHERE Id = @id";

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
                // Log the exception details for troubleshooting (implement logging as per your requirements)
                // Logger.LogError(ex, "Error al intentar eliminar el género con Id: {Id}", id);

                // Optionally, rethrow the exception or handle it as needed
                throw new ApplicationException("Se produjo un error al intentar eliminar el género.", ex);
            }
        }
        public int Modificacion(Slide entidad)
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
                    const string sql = @"UPDATE Slide 
                                        SET
                                            Nombre = @nombre,
                                            Descripcion = @descripcion,
                                            Imagen = @imagen,
                                            FechaUltimaModificacion = @fechaUltimaModificacion,
                                            Orden = @orden,
                                            Estado = @estado
                                        WHERE Id = @id";
                    
                    using (var command = new SqlCommand(sql, connection))
                    {
                        // Usar AddWithValue con cuidado: asegúrate de que el tipo de datos sea correcto
                        command.Parameters.AddWithValue("@nombre", entidad.Nombre ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@descripcion", entidad.Descripcion ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@fechaUltimaModificacion",entidad.FechaUltimaModificacion ?? (object)DateTime.Now);
                        command.Parameters.AddWithValue("@imagen", entidad.Imagen ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@orden", entidad.Orden ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@estado", entidad.Estado);
                        
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
                throw new ApplicationException("Se produjo un error al intentar modificar el slide.", ex);
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

        public IList<Slide> ObtenerTodos()
        {
            const string sql = "SELECT Id, Nombre, Descripcion, FechaCreacion, FechaUltimaModificacion, Imagen, Orden, Estado, FechaEliminacion FROM Slide";

            var slides = new List<Slide>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var slide = new Slide
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Nombre = reader.IsDBNull(reader.GetOrdinal("Nombre")) ? null : reader.GetString(reader.GetOrdinal("Nombre")),
                                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
                                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                                FechaUltimaModificacion = reader.IsDBNull(reader.GetOrdinal("FechaUltimaModificacion")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("FechaUltimaModificacion")),
                                Imagen = reader.IsDBNull(reader.GetOrdinal("Imagen")) ? null : reader.GetString(reader.GetOrdinal("Imagen")),
                                Orden = reader.IsDBNull(reader.GetOrdinal("Orden")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Orden")),
                                Estado = reader.GetByte(reader.GetOrdinal("Estado")),
                                FechaEliminacion = reader.IsDBNull(reader.GetOrdinal("FechaEliminacion")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("FechaEliminacion"))
                            };

                            slides.Add(slide);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Manejo específico para errores de SQL
                throw new ApplicationException("Ocurrió un error al intentar obtener los slides.", ex);
            }
            catch (Exception ex)
            {
                // Manejo general de excepciones
                throw new ApplicationException("Ocurrió un error inesperado.", ex);
            }

            return slides;
        }



        public Slide ObtenerPorId(int id)
        {
            Slide entidad = null;

            // Usar `using` para manejar recursos de manera segura
            using (var connection = new SqlConnection(connectionString))
            {
                string sql = @"
            SELECT Id, Nombre, Descripcion, FechaUltimaModificacion, Imagen, Orden, Estado
            FROM Slide
            WHERE Id = @id";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    command.CommandType = CommandType.Text;

                    try
                    {
                        connection.Open();

                        // Usar `using` para asegurar que el lector se cierra correctamente
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                entidad = new Slide
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                    Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
                                    // Manejar valores nulos en `FechaUltimaModificacion`
                                    FechaUltimaModificacion = reader.IsDBNull(reader.GetOrdinal("FechaUltimaModificacion"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("FechaUltimaModificacion")),
                                    Imagen = reader.IsDBNull(reader.GetOrdinal("Imagen"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("Imagen")),
                                    Orden = reader.IsDBNull(reader.GetOrdinal("Orden"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("Orden")),
                                    Estado = reader.GetByte(reader.GetOrdinal("Estado")),
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Manejar la excepción, por ejemplo, registrar el error
                        // Log exception (ex) here
                        throw new Exception("An error occurred while retrieving the slide.", ex);
                    }
                }
            }

            return entidad;
        }

    }
}
