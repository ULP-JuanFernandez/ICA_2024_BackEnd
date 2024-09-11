using ICA.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ICA.Controllers
{
    public class SlidersController : Controller
    {
        private readonly IRepositorioSliders _irepositorio;
        private readonly RepositorioSliders _repositorio;
        private readonly IWebHostEnvironment environment;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _imageDirectory = @"E:\Imagenes";

        public SlidersController(IRepositorioSliders irepositorio, RepositorioSliders repositorio, IWebHostEnvironment environment)
        {
            _irepositorio = irepositorio;
            _repositorio = repositorio;
            this.environment = environment;
            _webHostEnvironment = environment;
        }
        // GET: SlidersController
        // GET: SlidersController
        public ActionResult Index()
        {
            var slides = _irepositorio.ObtenerTodos();
            return View(slides);
        }

        // GET: SlidersController/Details/5
        public ActionResult Details(int id)
        {
            var slide = _irepositorio.ObtenerPorId(id);
            if (slide == null)
            {
                return NotFound();
            }
            return View(slide);
        }

        // GET: SlidersController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SlidersController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Slide model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.ImagenFile != null && model.ImagenFile.Length > 0)
                    {
                        // Define el nombre del archivo y el camino completo donde se guardará la imagen
                        var fileName = Path.GetFileName(model.ImagenFile.FileName);
                        var filePath = Path.Combine(@"C:\SharedImages", fileName);

                        // Guarda el archivo en el directorio especificado
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImagenFile.CopyToAsync(stream);
                        }

                        // Asigna el camino relativo de la imagen al modelo
                        model.Imagen = "/SharedImages/" + fileName;
                    }

                    //model.FechaCreacion = DateTime.Now;
                    //model.FechaUltimaModificacion = DateTime.Now;

                    // Aquí deberías agregar el código para guardar el modelo en la base de datos
                    // Por ejemplo, si estás usando Entity Framework:
                    _irepositorio.Alta(model);
                    // await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Opcional: Puedes registrar el error en el log
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar los datos. " + ex.Message);
                    return View(model);
                }
            }

            return View(model);
        }

        // GET: SlidersController/Edit/5
        public ActionResult Edit(int id)
        {
            var slide = _irepositorio.ObtenerPorId(id);
            if (slide == null)
            {
                return NotFound();
            }
            return View(slide);
        }

        // POST: SlidersController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Slide s)
        {
            if (!ModelState.IsValid)
            {
                return View(s);
            }

            try
            {
                var slideOriginal = _irepositorio.ObtenerPorId(s.Id);
                if (slideOriginal == null)
                {
                    return NotFound();
                }

                // Si se ha cargado una nueva imagen
                if (s.ImagenFile != null && s.ImagenFile.Length > 0)
                {
                    // Guardar la nueva imagen
                    var fileName = Path.GetFileName(s.ImagenFile.FileName);
                    var filePath = Path.Combine(@"C:\SharedImages", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await s.ImagenFile.CopyToAsync(stream);
                    }

                    // Actualizar la propiedad Imagen con la nueva ruta
                    s.Imagen = "/SharedImages/" + fileName;
                }
                else
                {
                    // Mantener la imagen existente
                    s.Imagen = slideOriginal.Imagen;
                }

                // Actualizar otras propiedades
                s.FechaUltimaModificacion = DateTime.Now;

                // Llamar al método para modificar en el repositorio
                _irepositorio.Modificacion(s);

                TempData["Mensaje"] = "Datos guardados correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Manejar errores, si es necesario, añadir un mensaje a ModelState
                ModelState.AddModelError(string.Empty, "Ocurrió un error al actualizar los datos. " + ex.Message);
                return View(s);
            }
        }

        // GET: SlidersController/Delete/5
        public ActionResult Delete(int id)
        {
            var slide = _irepositorio.ObtenerPorId(id);
            if (slide == null)
            {
                return NotFound();
            }
            return View(slide);
        }

        // POST: SlidersController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            var result = _irepositorio.Baja(id);
            if (result > 0)
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}