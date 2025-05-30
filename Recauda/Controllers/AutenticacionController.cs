using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recauda.Interfaces;
using Recauda.Models.DTOs;
using System.Security.Claims;


namespace Recauda.Controllers
{
    public class AutenticacionController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AutenticacionController> _logger;

        public AutenticacionController(IAuthService authService, ILogger<AutenticacionController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Si ya está autenticado, redirigir
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginDto { UrlRetorno = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            _logger.LogInformation($"Intento de login para usuario: {model.Login}");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var usuario = await _authService.ValidarCredencialesAsync(model.Login, model.Clave);

                if (usuario == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                    _logger.LogWarning($"Login fallido para usuario: {model.Login}");
                    return View(model);
                }

                // Crear claims para el usuario
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Login),
                    new Claim("FullName", usuario.Nombre),
                    new Claim("RUT", usuario.RutCompleto),
                    new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "SinRol"),
                    new Claim("RolId", usuario.RolId.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RecordarMe,
                    ExpiresUtc = model.RecordarMe
                        ? DateTimeOffset.UtcNow.AddDays(30)
                        : DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties);

                _logger.LogInformation($"Login exitoso para usuario: {model.Login}");
                TempData["SuccessMessage"] = $"¡Bienvenido, {usuario.Nombre}!";

                // Redirigir a la URL solicitada o al home
                if (!string.IsNullOrEmpty(model.UrlRetorno) && Url.IsLocalUrl(model.UrlRetorno))
                {
                    return Redirect(model.UrlRetorno);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error durante el login para usuario: {model.Login}");
                ModelState.AddModelError(string.Empty, "Error interno del sistema. Intente nuevamente.");
                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userName = User.Identity?.Name;
            _logger.LogInformation($"Usuario cerrando sesión: {userName}");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["InfoMessage"] = "Sesión cerrada exitosamente.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize]
        public IActionResult CambiarClave()
        {
            return View(new CambiarClaveDto());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarClave(CambiarClaveDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var exito = await _authService.CambiarClaveAsync(usuarioId, model.ClaveActual, model.ClaveNueva);

                if (!exito)
                {
                    ModelState.AddModelError("ClaveActual", "La contraseña actual es incorrecta.");
                    return View(model);
                }

                TempData["SuccessMessage"] = "Contraseña cambiada exitosamente.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                ModelState.AddModelError(string.Empty, "Error interno del sistema.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}