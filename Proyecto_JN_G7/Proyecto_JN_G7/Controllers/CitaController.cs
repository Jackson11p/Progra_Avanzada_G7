using Microsoft.AspNetCore.Mvc;
using Proyecto_JN_G7.Models;
using Proyecto_JN_G7.Services;

namespace Proyecto_JN_G7.Controllers
{
    public class CitaController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _http;
        private readonly IUtilitarios _utilitarios;
        public CitaController(IConfiguration configuration, IHttpClientFactory http, IUtilitarios utilitarios)
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
        public IActionResult Registro(Citas model)
        {
            using (var http = _http.CreateClient())
            {
                http.BaseAddress = new Uri(_configuration.GetSection("Start:ApiUrl").Value!);
                var resultado = http.PostAsJsonAsync("api/Cita/Registro", model).Result;

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

        [HttpPost]
        public IActionResult RegistroCitaCliente(Citas model)
        {
            using (var http = _http.CreateClient())
            {
                http.BaseAddress = new Uri(_configuration.GetSection("Start:ApiUrl").Value!);
                var resultado = http.PostAsJsonAsync("api/Cita/RegistroCitaCliente", model).Result;

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

        //Carga la info de las citas
        [HttpGet]
        public IActionResult ConsultarCitasUsuario()
        {
            using (var http = _http.CreateClient())
            {
                // Obtiene el UsuarioID de la sesión de AccountController
                var UsuarioID = HttpContext.Session.GetString("UsuarioID");

                http.BaseAddress = new Uri(_configuration.GetSection("Start:ApiUrl").Value!);
                http.DefaultRequestHeaders.Add("Authorization", "Bearer " + HttpContext.Session.GetString("JWT"));

                var resultado = http.GetAsync("api/Cita/ConsultarCitasUsuario?UsuarioID=" + UsuarioID).Result;

                if (resultado.IsSuccessStatusCode)
                {
                    //Lista lo que obtiene CitasPorUsuarioUnificada
                    var datos = resultado.Content.ReadFromJsonAsync<RespuestaEstandar<List<CitasPorUsuarioUnificada>>>().Result;
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


        #endregion

        public IActionResult Index()
        {
            return View();
        }
    }
}
