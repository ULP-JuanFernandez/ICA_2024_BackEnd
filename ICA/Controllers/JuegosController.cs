using ICA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ICA.Controllers
{
    public class JuegosController : Controller
    {
        private readonly IRepositorioJuego _irepositorio;
        //private readonly RepositorioProyecto _repositorio;
        private readonly RepositorioGenero rGenero;
        private readonly RepositorioMateria rMateria;
        private readonly RepositorioEtiqueta rEtiqueta;
        private readonly ILogger<EtiquetasController> _logger;
        public JuegosController(IRepositorioJuego irepositorio, RepositorioProyecto repositorio, RepositorioGenero g, RepositorioMateria m, RepositorioEtiqueta e, ILogger<EtiquetasController> logger)
        {
            _irepositorio = irepositorio;
            //_repositorio = repositorio;
            rGenero = g;
            rMateria = m;
            rEtiqueta = e;
            _logger = logger;
        }
        private void CargarDatosViewBag()
        {
            ViewBag.VBGeneros = rGenero.ObtenerTodos(2);
            ViewBag.VBMaterias = rMateria.ObtenerTodos(2);
            ViewBag.VBEtiquetas = rEtiqueta.ObtenerTodos(2);
        }
        public ActionResult Index()
        {
            var lista = _irepositorio.ObtenerTodos();
            return View(lista);
        }

        // GET: JuegosController/Details/5
        public ActionResult Details(int id)
        {
            var entidad = _irepositorio.ObtenerPorId(id);
            if (entidad == null)
            {
                return NotFound(); // Manejo de error si no se encuentra el etiqueta
            }

            return View(entidad);
        }


        // GET: JuegosController/Create
        public ActionResult Create()
        {
            CargarDatosViewBag();
            return View();
        }

        // POST: JuegosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Juego entidad)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _irepositorio.Alta(entidad);
                    TempData["SuccessMessage"] = "El juego se creó correctamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar guardar el juego.");
                ModelState.AddModelError(string.Empty, "Se produjo un error al intentar guardar el juego.");
            }

            CargarDatosViewBag();
            return View(entidad);
        }

        // GET: JuegosController/Edit/5
        public ActionResult Edit(int id)
        {
            if (id <= 0)
            {
                return BadRequest(); // Retorno de error si el ID no es válido
            }

            var entidad = _irepositorio.ObtenerPorId(id);
            if (entidad == null)
            {
                return NotFound();
            }

            CargarDatosViewBag();
            return View(entidad);
        }

        // POST: JuegosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Juego entidad)
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
                    return NotFound();
                }

                entidadExistente.Video = entidad.Video;
                entidadExistente.Imagen = entidad.Imagen;
                entidadExistente.Titulo = entidad.Titulo;
                entidadExistente.Fecha = entidad.Fecha;
                entidadExistente.Descripcion = entidad.Descripcion;
                entidadExistente.Integrantes = entidad.Integrantes;
                entidadExistente.GeneroId = entidad.GeneroId;
                entidadExistente.EtiquetaId = entidad.EtiquetaId;
                entidadExistente.MateriaId = entidad.MateriaId;
                entidadExistente.Link = entidad.Link;
                entidadExistente.Estado = entidad.Estado;

                _irepositorio.Modificacion(entidadExistente);
                TempData["SuccessMessage"] = "El juego se actualizó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar actualizar el juego.");
                ModelState.AddModelError(string.Empty, "Se produjo un error al intentar actualizar el juego.");
            }

            CargarDatosViewBag();
            return View(entidad);
        }

        // GET: JuegosController/Delete/5
        public ActionResult Delete(int id)
        {
            var entidad = _irepositorio.ObtenerPorId(id);
            if (entidad == null)
            {
                return NotFound();
            }
            return View(entidad);
        }

        // POST: JuegosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                int result = _irepositorio.Baja(id);
                TempData[result > 0 ? "SuccessMessage" : "Error"] =
                    result > 0 ? "Película eliminada correctamente." : "No se encontró el juego para eliminar.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar eliminar el juego con ID {Id}.", id);
                TempData["Error"] = "Se produjo un error al intentar eliminar el juego.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
