using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Proyecto_JN_G7.Models;
using Proyecto_JN_G7.Services;
using Microsoft.AspNetCore.Http;

namespace Proyecto_JN_G7.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _http;
        private readonly IUtilitarios _utilitarios;
        public AccountController(IConfiguration configuration, IHttpClientFactory http, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _http = http;
            _utilitarios = utilitarios;
        }

        #region Registro
        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registro(Autenticacion model)
        {
            model.ContrasenaHash = _utilitarios.Encrypt(model.ContrasenaHash!);
            using (var http = _http.CreateClient())
            {
                http.BaseAddress = new Uri(_configuration.GetSection("Start:ApiUrl").Value!);
                var resultado = http.PostAsJsonAsync("api/Account/Registro", model).Result;

                if (resultado.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    var respuesta = resultado.Content.ReadFromJsonAsync<RespuestaEstandar>().Result;
                    ViewBag.Mensaje = respuesta?.Mensaje;
                    return View();
                }
            }
        }
        #endregion

        #region Iniciar Sesion
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Autenticacion model)
        {
            model.ContrasenaHash = _utilitarios.Encrypt(model.ContrasenaHash!);

            using (var http = _http.CreateClient())
            {
                http.BaseAddress = new Uri(_configuration.GetSection("Start:ApiUrl").Value!);
                var resultado = http.PostAsJsonAsync("api/Account/Login", model).Result;

                if (resultado.IsSuccessStatusCode)
                {
                    var usuario = resultado.Content.ReadFromJsonAsync<Autenticacion>().Result;

                    if (usuario != null)
                    {
                        HttpContext.Session.SetString("UsuarioID", usuario.UsuarioID.ToString());
                        HttpContext.Session.SetString("NombreCompleto", usuario.NombreCompleto ?? "");
                        HttpContext.Session.SetInt32("RolID", usuario.RolID);

                        return RedirectToAction("Index", "Home");
                    }
                }

                var respuesta = resultado.Content.ReadFromJsonAsync<RespuestaEstandar>().Result;
                ViewBag.Mensaje = respuesta?.Mensaje;
                return View();
            }
        }
        #endregion

        #region Recuperar Acceso

        [HttpGet]
        public IActionResult RecuperarAcceso()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RecuperarAcceso(Autenticacion model)
        {
            using (var http = _http.CreateClient())
            {
                http.BaseAddress = new Uri(_configuration.GetSection("Start:ApiUrl").Value!);
                var resultado = http.PostAsJsonAsync("api/Account/RecuperarAcceso", model).Result;

                if (resultado.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    var respuesta = resultado.Content.ReadFromJsonAsync<RespuestaEstandar>().Result;
                    ViewBag.Mensaje = respuesta?.Mensaje;
                    return View();
                }
            }
        }

        #endregion

        #region Cerrar Sesion
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}
