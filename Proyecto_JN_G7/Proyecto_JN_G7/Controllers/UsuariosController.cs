using Microsoft.AspNetCore.Mvc;
using Proyecto_JN_G7.Models;
using Proyecto_JN_G7.Services;
using static System.Net.WebRequestMethods;

namespace Proyecto_JN_G7.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _http;
        private readonly IUtilitarios _utilitarios;
        public UsuariosController(IConfiguration configuration, IHttpClientFactory http, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _http = http;
            _utilitarios = utilitarios;
        }

        #region ActualizarPerfilUsuario

        [HttpGet]
        public IActionResult ActualizarPerfilUsuario()
        {
            using (var http = _http.CreateClient())
            {
                var UsuarioID = HttpContext.Session.GetString("UsuarioID");

                http.BaseAddress = new Uri(_configuration.GetSection("Start:ApiUrl").Value!);
                //http.DefaultRequestHeaders.Add("Authorization", "Bearer " + HttpContext.Session.GetString("JWT"));
                var resultado = http.GetAsync("api/Usuarios/ConsultarUsuario?UsuarioID=" + UsuarioID).Result;

                if (resultado.IsSuccessStatusCode)
                {
                    var datos = resultado.Content.ReadFromJsonAsync<RespuestaEstandar<Autenticacion>>().Result;
                    return View(datos?.Contenido!);
                }
                else
                {
                    var respuesta = resultado.Content.ReadFromJsonAsync<RespuestaEstandar>().Result;
                    ViewBag.Mensaje = respuesta?.Mensaje;
                    return View();
                }
            }
        }

        [HttpPost]
        public IActionResult ActualizarPerfilUsuario(Autenticacion model)
        {
            using (var http = _http.CreateClient())
            {
                var UsuarioID = HttpContext.Session.GetString("UsuarioID");
                model.UsuarioID = long.Parse(UsuarioID!);

                http.BaseAddress = new Uri(_configuration.GetSection("Start:ApiUrl").Value!);
                //http.DefaultRequestHeaders.Add("Authorization", "Bearer " + HttpContext.Session.GetString("JWT"));
                var resultado = http.PutAsJsonAsync("api/Usuarios/ActualizarUsuario", model).Result;

                if (resultado.IsSuccessStatusCode)
                {
                    HttpContext.Session.SetString("NombreCompleto", model.NombreCompleto!);
                    return RedirectToAction("Index", "Home");
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

        #region ActualizarContrasenna
        [HttpGet]
        public IActionResult ActualizarContrasenna()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ActualizarContrasenna(Autenticacion model)
        {
            if (model.ContrasenaHash != model.ConfirmarContrasenna)
            {
                ViewBag.Mensaje = "Las contraseñas no coinciden.";
                return View();
            }

            model.ContrasenaHash = _utilitarios.Encrypt(model.ContrasenaHash!);

            using (var http = _http.CreateClient())
            {
                var UsuarioID = HttpContext.Session.GetString("UsuarioID");
                model.UsuarioID = long.Parse(UsuarioID!);

                http.BaseAddress = new Uri(_configuration.GetSection("Start:ApiUrl").Value!);
                //http.DefaultRequestHeaders.Add("Authorization", "Bearer " + HttpContext.Session.GetString("JWT"));
                var resultado = http.PutAsJsonAsync("api/Usuarios/ActualizarContrasenna", model).Result;

                if (resultado.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Home");
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
    }
}
