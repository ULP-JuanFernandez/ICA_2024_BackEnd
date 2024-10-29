using ICA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ICA.Controllers
{
    public class EtiquetasController : Controller
    {
        private readonly IRepositorioEtiqueta _irepositorio;
        private readonly RepositorioEtiqueta _repositorio;
        private readonly IRepositorioTecnicatura _irepositorioT;
        private readonly ILogger<EtiquetasController> _logger;
        public EtiquetasController(IRepositorioEtiqueta irepositorio, IRepositorioTecnicatura it , RepositorioEtiqueta repositorio, ILogger<EtiquetasController> logger)
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

        // GET: EtiquetasController/Index
        public ActionResult Index()
        {
            var lista = _repositorio.ObtenerTodos();
            return View(lista);
        }

        // GET: EtiquetasController/Details/5
        public ActionResult Details(int id)
        {
            var entidad = _repositorio.ObtenerPorId(id);
            if (entidad == null)
            {
                TempData["Error"] = "No se encontró la etiqueta especificada.";
                return NotFound();
            }

            return View(entidad);
        }

        // GET: EtiquetasController/Create
        public ActionResult Create()
        {
            CargarDatosViewBag();
            return View();
        }

        // POST: EtiquetasController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Etiqueta etiqueta)
        {
            if (!ModelState.IsValid)
            {
                CargarDatosViewBag();
                return View(etiqueta);
            }

            try
            {
                _repositorio.Alta(etiqueta);
                TempData["SuccessMessage"] = "La etiqueta se creó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar guardar la etiqueta");
                ModelState.AddModelError(string.Empty, "Se produjo un error al intentar guardar la etiqueta.");
                CargarDatosViewBag();
                return View(etiqueta);
            }
        }

        // GET: EtiquetasController/Edit/5
        public ActionResult Edit(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "ID de etiqueta no válido.";
                return BadRequest();
            }

            var entidad = _repositorio.ObtenerPorId(id);
            if (entidad == null)
            {
                TempData["Error"] = "No se encontró la etiqueta especificada.";
                return NotFound();
            }

            CargarDatosViewBag();
            return View(entidad);
        }

        // POST: EtiquetasController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Etiqueta entidad)
        {
            if (!ModelState.IsValid)
            {
                CargarDatosViewBag();
                return View(entidad);
            }

            try
            {
                var entidadExistente = _repositorio.ObtenerPorId(id);
                if (entidadExistente == null)
                {
                    TempData["Error"] = "No se encontró la etiqueta especificada.";
                    return NotFound();
                }

                // Actualiza solo los campos necesarios
                entidadExistente.Nombre = entidad.Nombre;
                entidadExistente.Descripcion = entidad.Descripcion;
                entidadExistente.Estado = entidad.Estado;
                entidadExistente.TecnicaturaId = entidad.TecnicaturaId;

                _repositorio.Modificacion(entidadExistente);
                TempData["SuccessMessage"] = "La etiqueta se actualizó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar actualizar la etiqueta");
                ModelState.AddModelError(string.Empty, "Se produjo un error al intentar actualizar la etiqueta.");
                CargarDatosViewBag();
                return View(entidad);
            }
        }

        // GET: EtiquetasController/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                var entidad = _repositorio.ObtenerPorId(id);
                if (entidad == null)
                {
                    return NotFound();
                }
                return View(entidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar obtener la entidad con ID {Id}.", id);
                return View("Error", new ErrorViewModel { Message = "No se pudo recuperar la etiqueta." });
            }
        }

        // POST: EtiquetasController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                int result = _repositorio.Baja(id);
                TempData[result > 0 ? "SuccessMessage" : "Error"] =
                    result > 0 ? "Etiqueta eliminada correctamente." : "No se encontró la etiqueta para eliminar.";
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Intento de eliminar etiqueta fallido: {Id}", id);
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar eliminar la etiqueta con ID {Id}.", id);
                TempData["Error"] = "Se produjo un error al intentar eliminar la etiqueta.";
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
