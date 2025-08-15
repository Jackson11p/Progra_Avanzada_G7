using Microsoft.AspNetCore.Mvc;
using Proyecto_JN_G7.Models;
using Proyecto_JN_G7.Services;

namespace Proyecto_JN_G7.Controllers
{
    public class DoctorController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _http;
        private readonly IUtilitarios _utilitarios;
        public DoctorController(IConfiguration configuration, IHttpClientFactory http, IUtilitarios utilitarios)
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
        public IActionResult Registro(Doctores model)
        {            
            using (var http = _http.CreateClient())
            {
                http.BaseAddress = new Uri(_configuration.GetSection("Start:ApiUrl").Value!);
                var resultado = http.PostAsJsonAsync("api/Doctor/Registro", model).Result;

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
        public IActionResult Actualizar(Doctores model)
        {
            using var http = _http.CreateClient();
            http.BaseAddress = new Uri(_configuration.GetSection("Start:ApiUrl").Value!);

            var resultado = http.PutAsJsonAsync("api/Doctor/Actualizar", model).Result;

            if (resultado.IsSuccessStatusCode)
                return RedirectToAction("Index", "Home");
            else
            {
                var respuesta = resultado.Content.ReadFromJsonAsync<RespuestaEstandar>().Result;
                ViewBag.Mensaje = respuesta?.Mensaje;
                return RedirectToAction("Index", "Home");
            }
        }


        #endregion

        public IActionResult Index()
        {
            return View();
        }
    }
}
