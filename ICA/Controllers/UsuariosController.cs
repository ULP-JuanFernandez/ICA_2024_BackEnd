using ICA.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace ICA.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IRepositorioUsuario _irepositorio;
        private readonly RepositorioUsuario _repositorio;

        public UsuariosController(IRepositorioUsuario irepositorio, RepositorioUsuario repositorio)
        {
            _irepositorio = irepositorio;
            _repositorio = repositorio;
        }

        // GET: UsuariosController
        public ActionResult Index()
        {
            var lista = _irepositorio.ObtenerTodos();
            return View(lista);
        }

        // GET: UsuariosController1/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UsuariosController1/Create
        public ActionResult Create()
        {
            
            return View();
        }

        // POST: UsuariosController1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Usuario u)
        {
           
            if (!ModelState.IsValid)
            {
                
                return View(u);
            }

            try
            {
                // Generar un salt único para cada usuario
                byte[] salt = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(salt);
                }

                // Hash de la contraseña
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: u.Clave,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256, // Usar HMACSHA256
                    iterationCount: 10000, // Aumentar el número de iteraciones
                    numBytesRequested: 256 / 8));

                // Almacenar el salt y el hash
                u.Clave = hashed;
                u.Salt = salt;

                // Registrar el nuevo usuario
                int res = _irepositorio.Alta(u);             

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Manejo de excepciones...
                
                ModelState.AddModelError("", "Ocurrió un error al crear el usuario. Inténtalo de nuevo.");
                return View(u);
            }
        }

        
        // GET: UsuariosController/Edit/5
        public ActionResult Edit(int id)
        {
            var usuario = _repositorio.ObtenerPorId(id);
            var model = new UsuarioEditViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                Estado = usuario.Estado
            };
            
            if (model == null)
            {
                return NotFound(); // Manejar el caso donde no se encuentra el usuario 404
            }
            return View(model);
        }

        // POST: UsuariosController/Edit/5   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, UsuarioEditViewModel usuario)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si el nuevo correo ya existe en otro usuario
                    var existeCorreo = _repositorio.ExisteCorreo(usuario.Correo, id);
                    if (existeCorreo)
                    {
                        ModelState.AddModelError("Correo", "El correo electrónico ya está en uso por otro usuario.");
                        return View(usuario); // Regresar con error
                    }

                    // Llamar al método de modificación en el repositorio
                    var resultado = _repositorio.Modificacion(usuario);
                    if (resultado > 0)
                    {
                        return RedirectToAction(nameof(Index)); // Redirigir a la lista de usuarios
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error al actualizar el usuario."); // Agregar error si no se actualizó
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}"); // Manejar excepciones
                }
            }
            return View(usuario); // Si hay errores, regresar al formulario con los datos actuales
        }

        // GET: UsuariosController1/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UsuariosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
