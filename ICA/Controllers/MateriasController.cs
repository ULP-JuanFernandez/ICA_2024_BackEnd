using ICA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ICA.Controllers
{
    public class MateriasController : Controller
    {
        private readonly IRepositorioMateria _irepositorio;
        private readonly RepositorioMateria _repositorio;
        private readonly IRepositorioTecnicatura _irepositorioT;
        private readonly ILogger<MateriasController> _logger;

        public MateriasController(IRepositorioMateria irepositorio, IRepositorioTecnicatura it, RepositorioMateria repositorio, ILogger<MateriasController> logger)
        {
            _irepositorio = irepositorio;
            _irepositorioT = it;
            _repositorio = repositorio;
            _logger = logger;
        }
        // Método privado para cargar datos en ViewBag
        private void CargarDatosViewBag()
        {
            ViewBag.VBTecnicaturas = _irepositorioT.ObtenerTodos();
        }

        // GET: MateriasController/Index
        public ActionResult Index()
        {
            var lista = _irepositorio.ObtenerTodos();
            return View(lista);
        }

        // GET: MateriasController/Details/5
        public ActionResult Details(int id)
        {
            var entidad = _irepositorio.ObtenerPorId(id);
            if (entidad == null)
            {
                TempData["Error"] = "No se encontró la Materia especificada.";
                return NotFound();
            }

            return View(entidad);
        }

        // GET: MateriasController/Create
        public ActionResult Create()
        {
            CargarDatosViewBag();
            return View();
        }

        // POST: MateriasController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Materia materia)
        {
            if (!ModelState.IsValid)
            {
                CargarDatosViewBag();
                return View(materia);
            }

            try
            {
                _irepositorio.Alta(materia);
                TempData["SuccessMessage"] = "La Materia se creó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar guardar la Materia");
                ModelState.AddModelError(string.Empty, "Se produjo un error al intentar guardar la Materia.");
                CargarDatosViewBag();
                return View(materia);
            }
        }

        // GET: MateriasController/Edit/5
        public ActionResult Edit(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "ID de materia no válido.";
                return BadRequest();
            }

            var entidad = _irepositorio.ObtenerPorId(id);
            if (entidad == null)
            {
                TempData["Error"] = "No se encontró la Materia especificada.";
                return NotFound();
            }

            CargarDatosViewBag();
            return View(entidad);
        }

        // POST: MateriasController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Materia entidad)
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
                    TempData["Error"] = "No se encontró la Materia especificada.";
                    return NotFound();
                }

                // Actualiza solo los campos necesarios
                entidadExistente.Nombre = entidad.Nombre;

                entidadExistente.Estado = entidad.Estado;
                entidadExistente.TecnicaturaId = entidad.TecnicaturaId;

                _irepositorio.Modificacion(entidadExistente);
                TempData["SuccessMessage"] = "La Materia se actualizó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar actualizar la Materia");
                ModelState.AddModelError(string.Empty, "Se produjo un error al intentar actualizar la Materia.");
                CargarDatosViewBag();
                return View(entidad);
            }
        }

        // GET: MateriasController/Delete/5
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
                return View("Error", new ErrorViewModel { Message = "No se pudo recuperar la Materia." });
            }
        }

        // POST: MateriasController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                int result = _irepositorio.Baja(id);
                TempData[result > 0 ? "SuccessMessage" : "Error"] =
                    result > 0 ? "Materia eliminada correctamente." : "No se encontró la materia para eliminar.";
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Intento de eliminar materia fallido: {Id}", id);
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar eliminar la Materia con ID {Id}.", id);
                TempData["Error"] = "Se produjo un error al intentar eliminar la Materia.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
