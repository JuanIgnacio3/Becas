using System.Diagnostics;
using Becas.Models;
using Microsoft.AspNetCore.Mvc;

namespace Becas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        private static readonly List<string> _cantones = new()
        {
            "Cartago", "Paraíso", "La Guardia", "Turrialba", "Alvarado", "Oreamuno", "El Jardín", "Cervantes"
        };

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Cantones = _cantones;
            return View(new SolicitudBeca());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(SolicitudBeca model)
        {
            ViewBag.Cantones = _cantones;
            ViewBag.Submitted = true;
            ViewBag.Ok = ModelState.IsValid;
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
