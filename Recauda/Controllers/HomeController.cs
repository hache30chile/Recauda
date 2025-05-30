using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recauda.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace Recauda.Controllers
{
    [Authorize] 
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Pasar información del usuario a la vista
            ViewBag.UsuarioNombre = User.FindFirstValue("FullName");
            ViewBag.UsuarioLogin = User.Identity?.Name;
            ViewBag.UsuarioRol = User.FindFirstValue(ClaimTypes.Role);

            return View();
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