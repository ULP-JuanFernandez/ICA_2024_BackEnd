using ICA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ICA.Controllers
{
    public class ProyectoController : Controller
    {

        private readonly IRepositorioProyecto _irepositorio;
        private readonly RepositorioProyecto _repositorio;
        private readonly RepositorioGenero rGenero;
        private readonly RepositorioTecnicatura rTecnicatura;
        private readonly RepositorioEtiqueta rEtiqueta;
        public ProyectoController(IRepositorioProyecto irepositorio, RepositorioProyecto repositorio, RepositorioGenero g, RepositorioTecnicatura t, RepositorioEtiqueta e)
        {
            _irepositorio = irepositorio;
            _repositorio = repositorio;
            rGenero = g;
            rTecnicatura = t;
            rEtiqueta = e;
        }
        // GET: ProyectoController

        public ActionResult Index(int? filtroTecnicatura)
        {
            IList<Proyecto> lista;

            if (filtroTecnicatura.HasValue)
            {
                lista = _repositorio.ObtenerFiltroPorTecnicaturaId(filtroTecnicatura.Value);
            }
            else
            {
                lista = _repositorio.ObtenerTodos();
            }

            // Obtener la lista de tecnicaturas para mostrar en la vista
            ViewBag.VBTecnicaturas = rTecnicatura.ObtenerTodos();
            ViewBag.FiltroTecnicatura = filtroTecnicatura; // Pasar el filtro actual a la vista

            return View(lista);
        }
        //public ActionResult Index(int? filtroTecnicatura)
        //{
        //    IList<Proyecto> lista;

        //    if (filtroTecnicatura.HasValue)
        //    {
        //        lista = _repositorio.ObtenerFiltroPorTecnicaturaId(filtroTecnicatura.Value);
        //    }
        //    else
        //    {
        //        lista = _repositorio.ObtenerTodos();
        //    }





        //    // Obtener la lista de tecnicaturas para mostrar en la vista
        //    ViewBag.VBTecnicaturas = rTecnicatura.ObtenerTodos();

        //    return View(lista);
        //}

        // GET: ProyectoController/Details/5
        public ActionResult Details(int id)
        {
            var proyecto = _irepositorio.ObtenerPorId(id);
            if (proyecto == null)
            {
                return NotFound();
            }
            return View(proyecto);
        }

        // GET: ProyectoController/Create
        public ActionResult Create()
        {
            ViewBag.VBGeneros = rGenero.ObtenerTodos();
            ViewBag.VBEtiquetas = rEtiqueta.ObtenerTodos();
            return View();
        }

        // POST: ProyectoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Proyecto proyecto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Intenta guardar el nuevo género
                    _irepositorio.Alta(proyecto);

                    // Usa TempData para pasar el Id del nuevo género a la siguiente acción
                    TempData["SuccessMessage"] = "El proyecto se creó correctamente.";
                    TempData["Id"] = proyecto.Id;

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                   
                    ViewBag.VBGeneros = rGenero.ObtenerTodos();
                    ViewBag.VBTecnicaturas = rTecnicatura.ObtenerTodos();
                    return View(proyecto);
                }
            }
            catch (Exception ex)
            {
                // Manejo del error: registra el error y muestra un mensaje amigable
                // Aquí podrías registrar el error en un log
                ModelState.AddModelError(string.Empty, "Se produjo un error al intentar guardar el género.");
                
                ViewBag.VBGeneros = rGenero.ObtenerTodos();
                ViewBag.VBEtiquetas = rEtiqueta.ObtenerTodos();
                return View(proyecto);
            }
        }

        // GET: ProyectoController/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var entidad = _irepositorio.ObtenerPorId(id);
                ViewBag.VBGeneros = rGenero.ObtenerTodos();
                ViewBag.VBEtiquetas = rEtiqueta.ObtenerTodos();
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

        // POST: ProyectoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Proyecto entidad)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.VBGeneros = rGenero.ObtenerTodos();
                ViewBag.VBEtiquetas = rEtiqueta.ObtenerTodos();
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
                    ViewBag.VBGeneros = rGenero.ObtenerTodos();
                    ViewBag.VBEtiquetas = rEtiqueta.ObtenerTodos();
                    return View(entidad);
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones: registrar y mostrar un mensaje de error general
                // Ejemplo de registro de error:
                // _logger.LogError(ex, "Error al intentar guardar la entidad.");
                TempData["Error"] = "Se produjo un error al intentar guardar los datos.";
                ViewBag.VBGeneros = rGenero.ObtenerTodos();
                ViewBag.VBEtiquetas = rEtiqueta.ObtenerTodos();
                return View(entidad);
            }
        }

        // GET: ProyectoController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProyectoController/Delete/5
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

        //---------------------------Prueba--------------------------

        //public ActionResult CreateGenero(int id, Genero genero)
        //{
        //    var entidad = _irepositorio.ObtenerPorId(id);
        //    try
        //    {
                
        //        ViewBag.VBGeneros = rGenero.ObtenerTodos();
        //        ViewBag.VBTecnicaturas = rTecnicatura.ObtenerTodos();

        //        if (ModelState.IsValid)
        //        {
        //            // Intenta guardar el nuevo género
        //            rGenero.Alta(genero);

        //            // Usa TempData para pasar el Id del nuevo género a la siguiente acción
        //            TempData["SuccessMessage"] = "El género se creó correctamente.";
        //            TempData["Id"] = genero.Id;

        //            return RedirectToAction(nameof(Create));
        //        }
        //        else
        //        {

        //            ViewBag.VBGeneros = rGenero.ObtenerTodos();
        //            ViewBag.VBTecnicaturas = rTecnicatura.ObtenerTodos();
        //            return View(entidad);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Manejo del error: registra el error y muestra un mensaje amigable
        //        // Aquí podrías registrar el error en un log
        //        ModelState.AddModelError(string.Empty, "Se produjo un error al intentar guardar el género.");

        //        ViewBag.VBGeneros = rGenero.ObtenerTodos();
        //        ViewBag.VBTecnicaturas = rTecnicatura.ObtenerTodos();
        //        return View(entidad);
        //    }
        //}
    }
}
