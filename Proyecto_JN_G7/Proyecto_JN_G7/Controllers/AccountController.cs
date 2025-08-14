using Microsoft.AspNetCore.Mvc;
using Proyecto_JN_G7.Models;
using Proyecto_JN_G7.Services;
using System.Net.Http.Json;

namespace Proyecto_JN_G7.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IUtilitarios _utilitarios;

        public AccountController(IHttpClientFactory http, IUtilitarios utilitarios)
        {
            _http = http;
            _utilitarios = utilitarios;
        }

        #region Registro
        [HttpGet]
        public IActionResult Registro() => View();

        [HttpPost]
        public async Task<IActionResult> Registro(Autenticacion model)
        {
            model.ContrasenaHash = _utilitarios.Encrypt(model.ContrasenaHash!);

            var client = _http.CreateClient("Api");
            var resp = await client.PostAsJsonAsync("api/Account/Registro", model);

            if (resp.IsSuccessStatusCode)
                return RedirectToAction("Login", "Account");

            var respuesta = await resp.Content.ReadFromJsonAsync<RespuestaEstandar>();
            ViewBag.Mensaje = respuesta?.Mensaje;
            return View();
        }
        #endregion

        #region Iniciar Sesión (usando tu endpoint actual)
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(Autenticacion model)
        {
            
            model.ContrasenaHash = _utilitarios.Encrypt(model.ContrasenaHash!);

            var client = _http.CreateClient("Api");
            var resp = await client.PostAsJsonAsync("api/Account/Login", model);

            var contentType = resp.Content.Headers.ContentType?.MediaType ?? "";

            if (!resp.IsSuccessStatusCode)
            {
                if (contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                {
                    var respuesta = await resp.Content.ReadFromJsonAsync<RespuestaEstandar>();
                    ViewBag.Mensaje = respuesta?.Mensaje ?? "Credenciales inválidas";
                }
                else
                {
                    var raw = await resp.Content.ReadAsStringAsync();
                    ViewBag.Mensaje = string.IsNullOrWhiteSpace(raw) ? "Credenciales inválidas" : raw;
                }
                return View();
            }

            var usuario = await resp.Content.ReadFromJsonAsync<Autenticacion>(
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (usuario == null)
            {
                ViewBag.Mensaje = "No se pudo obtener la información del usuario.";
                return View();
            }

            HttpContext.Session.SetString("UsuarioID", usuario.UsuarioID.ToString());
            HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto ?? "");
            HttpContext.Session.SetInt32("RolID", usuario.RolID);

            var rolNombre = usuario.RolID == 3 ? "Administrador"
                         : usuario.RolID == 2 ? "Doctor"
                         : "Usuario";
            HttpContext.Session.SetString("ROL", rolNombre);

            if (rolNombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Recuperar Acceso
        [HttpGet]
        public IActionResult RecuperarAcceso() => View();

        [HttpPost]
        public async Task<IActionResult> RecuperarAcceso(Autenticacion model)
        {
            if (!string.IsNullOrEmpty(model.ContrasenaHash))
                model.ContrasenaHash = _utilitarios.Encrypt(model.ContrasenaHash);

            var client = _http.CreateClient("Api");
            var resp = await client.PostAsJsonAsync("api/Account/RecuperarAcceso", model);

            if (resp.IsSuccessStatusCode)
                return RedirectToAction("Login", "Account");

            var respuesta = await resp.Content.ReadFromJsonAsync<RespuestaEstandar>();
            ViewBag.Mensaje = respuesta?.Mensaje;
            return View();
        }
        #endregion

        #region Cerrar Sesión
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}
