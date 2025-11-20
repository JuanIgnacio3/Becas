using Becas.Data;
using Becas.Identity;
using Becas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;


namespace Becas.Controllers
{
    public class BecaController : Controller
    {
        private readonly ISolicitudBecaRepository _repo;
        private readonly InMemoryUserStore _user;

        public BecaController(ISolicitudBecaRepository repo, InMemoryUserStore user)
        {
            _repo = repo;
            _user = user;

        }

        private void CargarCantones()
        {
            ViewBag.Cantones = new List<string>
            {
                "Cartago", "Paraíso", "La Guardia", "Turrialba", "Alvarado", "Oreamuno", "El Jardín", "Cervantes"
            };
        }

        // Vista principal que redirige al formulario
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Create));
        }

        // GET: /Beca/Create
        [HttpGet]
        public IActionResult Create()
        {
            CargarCantones();
            return View(new SolicitudBeca());
        }

        // POST: /Beca/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SolicitudBeca model)
        {
            CargarCantones();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _repo.Add(model);
            TempData["Mensaje"] = "Solicitud enviada correctamente. ¡Gracias!";
            return RedirectToAction(nameof(Create));
        }

        // GET: /Beca/ListaSolicitudes
        [HttpGet]
        [Authorize(Roles = "Administrador,Revisor")] //solo el admin y revisor podrán verlo
        public IActionResult ListaSolicitudes()
        {
            var identity = User.Identity as ClaimsIdentity;

            // Información básica
            var userName = User.Identity?.Name;
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

            // Claims comunes de Identity
            var userId = identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = identity?.FindFirst(ClaimTypes.Email)?.Value;

            // Todos los claims en una lista/diccionario
            var claims = identity?.Claims
                .Select(c => new { Tipo = c.Type, Valor = c.Value })
                .ToList();

            // Roles (también son claims)
            if (identity?.IsAuthenticated == true)
            {
                var roles = identity.FindAll(identity.RoleClaimType)
                                    .Select(c => c.Value)
                                    .ToList();

            }

            var data = _repo.GetAll();
            return View("Lista", data);
        }

        // GET: /Beca/Ejemplos
        [HttpGet]
        public IActionResult Ejemplos()
        {
            CargarCantones();
            return View();
        }
    }
}

