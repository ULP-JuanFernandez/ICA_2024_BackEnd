using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Security.Cryptography;

namespace ICA.Models
{
    public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
    {
        private readonly string connectionString;
        private readonly IConfiguration configuration;

        public RepositorioUsuario(IConfiguration configuration) : base(configuration)
        {
            this.configuration = configuration;
            connectionString = configuration["ConnectionStrings:DefaultConnection"];
        }
        public int Alta(Usuario u)
        {
            int res = -1;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = @"
            INSERT INTO Usuario (Nombre, Apellido, Correo, Rol, Clave, Salt, FechaCreacion, FechaModificacion, Estado) 
            VALUES (@nombre, @apellido, @correo, @rol, @clave, @salt, GETDATE(), GETDATE(), @estado);
            SELECT SCOPE_IDENTITY();"; // Devuelve el ID insertado

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@nombre", u.Nombre);
                    command.Parameters.AddWithValue("@apellido", u.Apellido);
                    command.Parameters.AddWithValue("@correo", u.Correo);
                    command.Parameters.AddWithValue("@rol", u.Rol);
                    command.Parameters.AddWithValue("@clave", u.Clave);
                    command.Parameters.AddWithValue("@salt", u.Salt); // Ahora es VARBINARY
                    command.Parameters.AddWithValue("@estado", u.Estado);

                    try
                    {
                        connection.Open();
                        res = Convert.ToInt32(command.ExecuteScalar());
                        u.Id = res;
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception("Error al insertar el usuario: " + ex.Message);
                    }
                }
            }

            return res;
        }

        public IList<Usuario> ObtenerTodos()
        {
            var usuarios = new List<Usuario>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT Id, Nombre, Apellido, Correo, Rol, Clave, Salt, FechaCreacion, FechaModificacion, Estado FROM Usuario";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var usuario = new Usuario
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Correo = reader.GetString(3),
                                Rol = reader.GetString(4),
                                Clave = reader.GetString(5),
                                Salt = (byte[])reader[6], // Asumiendo que el Salt es VARBINARY
                                FechaCreacion = reader.GetDateTime(7),
                                FechaModificacion = reader.GetDateTime(8),
                                Estado = reader.GetByte(9)
                            };
                            usuarios.Add(usuario);
                        }
                    }
                }
            }

            return usuarios;
        }
        public Usuario ObtenerPorId(int id)
        {
            Usuario usuario = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT Id, Nombre, Apellido, Correo, Rol, Clave, Salt, FechaCreacion, FechaModificacion, Estado FROM Usuario WHERE Id = @id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Correo = reader.GetString(3),
                                Rol = reader.GetString(4),
                                Clave = reader.GetString(5), // Asegúrate de que esto sea seguro, ya que no deberías mostrar la clave
                                Salt = (byte[])reader[6],
                                FechaCreacion = reader.GetDateTime(7),
                                FechaModificacion = reader.GetDateTime(8),
                                Estado = reader.GetByte(9)
                            };
                        }
                    }
                }
            }

            return usuario; // Retorna el usuario encontrado o null si no se encontró
        }
        public int Baja(int id)
        {
            return 0;
        }
        public int Modificacion(Usuario u)
        {
            int resultado = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = "UPDATE Usuario SET Nombre = @nombre, Apellido = @apellido, Correo = @correo, " +
                                 "Rol = @rol, FechaModificacion = @fechaModificacion, Estado = @estado " +
                                 "WHERE Id = @id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@nombre", u.Nombre);
                        command.Parameters.AddWithValue("@apellido", u.Apellido);
                        command.Parameters.AddWithValue("@correo", u.Correo);
                        command.Parameters.AddWithValue("@rol", u.Rol);
                        command.Parameters.AddWithValue("@fechaModificacion", DateTime.UtcNow); // Actualizar la fecha de modificación
                        command.Parameters.AddWithValue("@estado", u.Estado);
                        command.Parameters.AddWithValue("@id", u.Id);

                        connection.Open();
                        resultado = command.ExecuteNonQuery(); // Devuelve el número de filas afectadas
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones (puedes registrarlas o lanzar una nueva excepción)
                Console.WriteLine($"Error al modificar el usuario: {ex.Message}");
            }

            return resultado;
        }
        public bool ExisteCorreo(string correo, int usuarioId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = "SELECT COUNT(*) FROM Usuario WHERE Correo = @correo AND Id <> @usuarioId";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@correo", correo);
                    command.Parameters.AddWithValue("@usuarioId", usuarioId);

                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0; // Retorna true si existe otro usuario con el mismo correo
                }
            }
        }
        public int ModificarClave(int id, string nuevaClave)
        {
            int resultado = 0;

            // Generar un nuevo salt
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            // Hash de la nueva contraseña
            string hashedClave = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: nuevaClave,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = "UPDATE Usuario SET Clave = @clave, Salt = @salt, FechaModificacion = @fechaModificacion WHERE Id = @id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@clave", hashedClave);
                        command.Parameters.AddWithValue("@salt", Convert.ToBase64String(salt)); // Guardar el salt
                        command.Parameters.AddWithValue("@fechaModificacion", DateTime.UtcNow); // Actualizar la fecha de modificación
                        command.Parameters.AddWithValue("@id", id);

                        connection.Open();
                        resultado = command.ExecuteNonQuery(); // Devuelve el número de filas afectadas
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                Console.WriteLine($"Error al modificar la clave del usuario: {ex.Message}");
            }

            return resultado;
        }

        internal int Modificacion(UsuarioEditViewModel usuario)
        {
            throw new NotImplementedException();
        }
    }
}