using ICA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

namespace ICA.Controllers
{
    public class GenerosController : Controller
    {
        private readonly IRepositorioGenero _irepositorio;
        private readonly IRepositorioTecnicatura _irepositorioT;
        private readonly ILogger<GenerosController> _logger;

        public GenerosController(IRepositorioGenero irepositorio, IRepositorioTecnicatura irepositorioT, ILogger<GenerosController> logger)
        {
            _irepositorio = irepositorio;
            _irepositorioT = irepositorioT;
            _logger = logger;
        }
        // Método privado para cargar datos en ViewBag
        private void CargarDatosViewBag()
        {
            ViewBag.VBTecnicaturas = _irepositorioT.ObtenerTodos();
        }

        // GET: GenerosController/Index
        public ActionResult Index()
        {
            var lista = _irepositorio.ObtenerTodos();
            return View(lista);
        }

        // GET: GenerosController/Details/5
        public ActionResult Details(int id)
        {
            var entidad = _irepositorio.ObtenerPorId(id);
            if (entidad == null)
            {
                TempData["Error"] = "No se encontró el genero especificada.";
                return NotFound();
            }

            return View(entidad);
        }

        // GET: GenerosController/Create
        public ActionResult Create()
        {
            CargarDatosViewBag();
            return View();
        }

        // POST: GenerosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Genero genero)
        {
            if (!ModelState.IsValid)
            {
                CargarDatosViewBag();
                return View(genero);
            }

            try
            {
                _irepositorio.Alta(genero);
                TempData["SuccessMessage"] = "El genero se creó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar guardar el genero");
                ModelState.AddModelError(string.Empty, "Se produjo un error al intentar guardar el genero.");
                CargarDatosViewBag();
                return View(genero);
            }
        }

        // GET: GenerosController/Edit/5
        public ActionResult Edit(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "ID de genero no válido.";
                return BadRequest();
            }

            var entidad = _irepositorio.ObtenerPorId(id);
            if (entidad == null)
            {
                TempData["Error"] = "No se encontró el genero especificada.";
                return NotFound();
            }

            CargarDatosViewBag();
            return View(entidad);
        }

        // POST: GenerosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Genero entidad)
        {
            if (!ModelState.IsValid)
            {
                CargarDatosViewBag();
                return View(entidad);
            }

            try
            {
                var entidadExistente = _irepositorio.ObtenerPorId(id);
                if (entidadExistente == null)
                {
                    TempData["Error"] = "No se encontró el genero especificada.";
                    return NotFound();
                }

                // Actualiza solo los campos necesarios
                entidadExistente.Nombre = entidad.Nombre;
                entidadExistente.Descripcion = entidad.Descripcion;
                entidadExistente.Estado = entidad.Estado;
                entidadExistente.TecnicaturaId = entidad.TecnicaturaId;

                _irepositorio.Modificacion(entidadExistente);
                TempData["SuccessMessage"] = "El genero se actualizó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar actualizar el genero");
                ModelState.AddModelError(string.Empty, "Se produjo un error al intentar actualizar el genero.");
                CargarDatosViewBag();
                return View(entidad);
            }
        }

        // GET: GenerosController/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                var entidad = _irepositorio.ObtenerPorId(id);
                if (entidad == null)
                {
                    return NotFound();
                }
                return View(entidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar obtener el entidad con ID {Id}.", id);
                return View("Error", new ErrorViewModel { Message = "No se pudo recuperar el genero." });
            }
        }

        // POST: GenerosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                int result = _irepositorio.Baja(id);
                TempData[result > 0 ? "SuccessMessage" : "Error"] =
                    result > 0 ? "Genero eliminad correctamente." : "No se encontró el genero para eliminar.";
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Intento de eliminar genero fallido: {Id}", id);
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar eliminar el genero con ID {Id}.", id);
                TempData["Error"] = "Se produjo un error al intentar eliminar el genero.";
            }

            return RedirectToAction(nameof(Index));
        }


    }
}