using ICA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ICA.Controllers
{
    public class HomeController : Controller
    {

        private readonly IRepositorioProyecto _irepositorio;
        private readonly RepositorioProyecto _repositorio;
        private readonly RepositorioGenero rGenero;
        private readonly RepositorioTecnicatura rTecnicatura;
        private readonly RepositorioSliders rSlides;
        public HomeController(IRepositorioProyecto irepositorio, RepositorioProyecto repositorio, RepositorioGenero g, RepositorioTecnicatura t, RepositorioSliders s)
        {
            _irepositorio = irepositorio;
            _repositorio = repositorio;
            rGenero = g;
            rTecnicatura = t;
            rSlides = s;
        }
        // GET: ProyectoController
        public ActionResult Index()
        {

            var lista = rSlides.ObtenerTodos();
           

            return View(lista);
        }

        // GET: ProyectoController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }
    }
}
