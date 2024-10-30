using ICA.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ICA.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IRepositorioUsuario _irepositorio;
        private readonly RepositorioUsuario _repositorio;
        private readonly ILogger<EtiquetasController> _logger;
        public UsuariosController(IRepositorioUsuario irepositorio, RepositorioUsuario repositorio, ILogger<EtiquetasController> logger)
        {
            _irepositorio = irepositorio;
            _repositorio = repositorio;
            _logger = logger;
        }

        // GET: UsuariosController
        public ActionResult Index()
        {
            var lista = _repositorio.ObtenerTodos();
            return View(lista);
        }

        // GET: UsuariosController/Details/5
        public ActionResult Details(int id)
        {
            var usuario = _repositorio.ObtenerPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // GET: UsuariosController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UsuariosController/Create
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
                byte[] salt = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: u.Clave,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000, // Mayor número de iteraciones para mayor seguridad
                    numBytesRequested: 256 / 8));

                u.Clave = hashed;
                u.EstablecerSalt(salt);

                int res = _repositorio.Alta(u);
                if (res > 0)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Error al registrar el usuario.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ocurrió un error: {ex.Message}");
            }
            return View(u);
        }

        // GET: UsuariosController/Edit/5
        public ActionResult Edit(int id)
        {
            var usuario = _repositorio.ObtenerPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }

            var model = new UsuarioEditViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Correo = usuario.Correo,
                Rol = usuario.Rol,
                Estado = usuario.Estado
            };

            return View(model);
        }

        // POST: UsuariosController/Edit/5   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, UsuarioEditViewModel usuario)
        {
            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            try
            {
                var existeCorreo = _repositorio.ExisteCorreo(usuario.Correo, id);
                if (existeCorreo)
                {
                    ModelState.AddModelError("Correo", "El correo electrónico ya está en uso por otro usuario.");
                    return View(usuario);
                }

                var usuarioToUpdate = new Usuario
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Correo = usuario.Correo,
                    Rol = usuario.Rol,
                    Estado = usuario.Estado
                };

                var resultado = _repositorio.Modificacion(usuarioToUpdate);
                if (resultado > 0)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Error al actualizar el usuario.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            return View(usuario);
        }

        // GET: UsuariosController/Delete/5
        public ActionResult Delete(int id)
        {
            var usuario = _repositorio.ObtenerPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: UsuariosController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var resultado = _repositorio.Baja(id);
                if (resultado > 0)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Error al eliminar el usuario.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            return View();
        }


        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(UsuarioLoginViewModel login)
        {
                if (!ModelState.IsValid)
            {
                return View(login);
            }

            try
            {
                var usuario = _repositorio.ObtenerPorEmail(login.Correo);
                if (usuario == null)
                {
                    ModelState.AddModelError("", "El email o clave no son correctos.");
                    return View(login);
                }

                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: login.Clave,
                    salt: usuario.Salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000, // Mayor número de iteraciones para mayor seguridad
                    numBytesRequested: 256 / 8));
                

                if (hashed != usuario.Clave)
                {
                    ModelState.AddModelError("", "El email o clave no son correctos.");
                    return View(login);
                }

                // Crear los claims del usuario autenticado
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Correo),
                    new Claim("Rol", usuario.Rol),
                    new Claim("FullName", $"{usuario.Nombre} {usuario.Apellido}"),
                    new Claim(ClaimTypes.Role, usuario.Rol),
            
                    new Claim("UsuarioId", usuario.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction(nameof(Index), "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ocurrió un error al iniciar sesión. Intente nuevamente.");
                // O podrías registrar el error para diagnosticarlo si fuera necesario
                // _logger.LogError(ex, "Error al intentar iniciar sesión");
                return View(login);
            }
        }

       
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Cierra la sesión del usuario actual
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Opcional: Registrar la actividad de cierre de sesión
                _logger.LogInformation("Usuario ha cerrado sesión.");

                // Redirige a la página de inicio
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Manejo de errores (opcional)
                _logger.LogError(ex, "Error al cerrar sesión.");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}