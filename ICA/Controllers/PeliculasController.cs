using ICA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ICA.Controllers
{
    public class PeliculasController : Controller
    {
        private readonly IRepositorioPelicula _irepositorio;
        //private readonly RepositorioProyecto _repositorio;
        private readonly RepositorioGenero rGenero;
        private readonly RepositorioMateria rMateria;
        private readonly RepositorioEtiqueta rEtiqueta;
        private readonly ILogger<EtiquetasController> _logger;
        public PeliculasController(IRepositorioPelicula irepositorio, RepositorioProyecto repositorio, RepositorioGenero g, RepositorioMateria m, RepositorioEtiqueta e, ILogger<EtiquetasController> logger)
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
            ViewBag.VBGeneros = rGenero.ObtenerTodos(1);
            ViewBag.VBMaterias = rMateria.ObtenerTodos(1);
            ViewBag.VBEtiquetas = rEtiqueta.ObtenerTodos(1);
        }
        public ActionResult Index()
        {
            var lista = _irepositorio.ObtenerTodos();
            return View(lista);
        }

        // GET: PeliculasController/Details/5
        public ActionResult Details(int id)
        {
            var entidad = _irepositorio.ObtenerPorId(id);
            if (entidad == null)
            {
                return NotFound(); // Manejo de error si no se encuentra la etiqueta
            }

            return View(entidad);
        }
        

        // GET: PeliculasController/Create
        public ActionResult Create()
        {
            CargarDatosViewBag();
            return View();
        }

        // POST: PeliculasController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Pelicula entidad)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _irepositorio.Alta(entidad);
                    TempData["SuccessMessage"] = "La película se creó correctamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar guardar la película.");
                ModelState.AddModelError(string.Empty, "Se produjo un error al intentar guardar la película.");
            }

            CargarDatosViewBag();
            return View(entidad);
        }

        // GET: PeliculasController/Edit/5
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

        // POST: PeliculasController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Pelicula entidad)
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
                entidadExistente.Creador = entidad.Creador;
                entidadExistente.Fecha = entidad.Fecha;
                entidadExistente.Sinopsis = entidad.Sinopsis;
                entidadExistente.Integrantes = entidad.Integrantes;
                entidadExistente.GeneroId = entidad.GeneroId;
                entidadExistente.EtiquetaId = entidad.EtiquetaId;
                entidadExistente.MateriaId = entidad.MateriaId;
                entidadExistente.Estado = entidad.Estado;

                _irepositorio.Modificacion(entidadExistente);
                TempData["SuccessMessage"] = "La película se actualizó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar actualizar la película.");
                ModelState.AddModelError(string.Empty, "Se produjo un error al intentar actualizar la película.");
            }

            CargarDatosViewBag();
            return View(entidad);
        }

        // GET: PeliculasController/Delete/5
        public ActionResult Delete(int id)
        {
            var entidad = _irepositorio.ObtenerPorId(id);
            if (entidad == null)
            {
                return NotFound();
            }
            return View(entidad);
        }

        // POST: PeliculasController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                int result = _irepositorio.Baja(id);
                TempData[result > 0 ? "SuccessMessage" : "Error"] =
                    result > 0 ? "Película eliminada correctamente." : "No se encontró la película para eliminar.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar eliminar la película con ID {Id}.", id);
                TempData["Error"] = "Se produjo un error al intentar eliminar la película.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
