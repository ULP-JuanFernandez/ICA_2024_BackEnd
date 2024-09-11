using ICA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ICA.Controllers
{
    public class TecnicaturaController : Controller
    {
        private readonly IRepositorioTecnicatura _irepositorio;
        private readonly RepositorioTecnicatura _repositorio;

        public TecnicaturaController(IRepositorioTecnicatura irepositorio, RepositorioTecnicatura repositorio)
        {
            _irepositorio = irepositorio;
            _repositorio = repositorio;
        }
        // GET: GeneroController
        public ActionResult Index()
        {
            var lista = _irepositorio.ObtenerTodos();

            return View(lista);
        }

        // GET: GeneroController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: GeneroController/Create
        public ActionResult Create()
        {
            try
            {
                ViewBag.Generos = _irepositorio.ObtenerTodos();
                return View();
            }
            catch (Exception ex)
            {
                // Registra el error y muestra una vista amigable para el usuario
                // Aquí podrías registrar el error en un log
                ModelState.AddModelError(string.Empty, "Se produjo un error al intentar cargar la página de creación.");
                return View(); // Opcionalmente, podrías redirigir a una vista de error genérica
            }
        }

        // POST: GeneroController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Tecnicatura tecnicatura)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Intenta guardar el nuevo género
                    _irepositorio.Alta(tecnicatura);

                    // Usa TempData para pasar el Id del nuevo género a la siguiente acción
                    TempData["SuccessMessage"] = "El género se creó correctamente.";
                    TempData["Id"] = tecnicatura.Id;

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Recarga la lista de géneros en caso de error de validación
                    ViewBag.Generos = _irepositorio.ObtenerTodos();
                    return View(tecnicatura);
                }
            }
            catch (Exception ex)
            {
                // Manejo del error: registra el error y muestra un mensaje amigable
                // Aquí podrías registrar el error en un log
                ModelState.AddModelError(string.Empty, "Se produjo un error al intentar guardar el género.");
                ViewBag.Generos = _irepositorio.ObtenerTodos();
                return View(tecnicatura);
            }
        }

        // GET: GeneroController/Edit/5
        public IActionResult Edit(int id)
        {
            try
            {
                var entidad = _repositorio.ObtenerPorId(id);

                if (entidad == null)
                {
                    // Retorna una respuesta 404 Not Found
                    TempData["Error"] = "El elemento solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }

                return View(entidad);
            }
            catch (Exception ex)
            {
                // Manejo de excepciones: registrar y mostrar un mensaje de error general
                // Ejemplo de registro de error:
                // _logger.LogError(ex, "Error al intentar cargar la entidad para edición.");
                TempData["Error"] = "Se produjo un error al intentar cargar la entidad para edición.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Tecnicatura/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Tecnicatura entidad)
        {
            if (!ModelState.IsValid)
            {
                // Si el modelo no es válido, vuelve a mostrar la vista con errores
                ViewBag.Infos = _repositorio.ObtenerTodos(); // Opcional: cargar datos necesarios para la vista
                return View(entidad);
            }

            try
            {
                entidad.Id = id;
                int resultado = _repositorio.Modificacion(entidad);

                if (resultado > 0)
                {
                    TempData["Mensaje"] = "Datos guardados correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Si no se actualizó ningún registro, muestra un mensaje de error
                    ModelState.AddModelError("", "No se pudo actualizar la entidad. Verifique los datos e intente nuevamente.");
                    ViewBag.Infos = _repositorio.ObtenerTodos(); // Opcional: cargar datos necesarios para la vista
                    return View(entidad);
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones: registrar y mostrar un mensaje de error general
                // Ejemplo de registro de error:
                // _logger.LogError(ex, "Error al intentar guardar la entidad.");
                TempData["Error"] = "Se produjo un error al intentar guardar los datos.";
                ViewBag.Infos = _repositorio.ObtenerTodos(); // Opcional: cargar datos necesarios para la vista
                return View(entidad);
            }
        }

        // GET: GeneroController/Delete/5
        public IActionResult Delete(int id)
        {
            var tecnicatura = _irepositorio.ObtenerPorId(id);

            if (tecnicatura == null)
            {
                TempData["Error"] = "No se encontró el género para eliminar.";
                return RedirectToAction(nameof(Index));
            }

            return View(tecnicatura);
        }

        // POST: Tecnicatura/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                int result = _irepositorio.Baja(id);

                if (result > 0)
                {
                    TempData["Mensaje"] = "Género eliminado correctamente.";
                }
                else
                {
                    TempData["Mensaje"] = "No se encontró el género para eliminar.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Registro del error puede ser implementado aquí (si se tiene un logger configurado)
                // Logger.LogError(ex, "Error al eliminar el género con Id: {Id}", id);

                TempData["Error"] = "Se produjo un error al intentar eliminar el género.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

