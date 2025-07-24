using Microsoft.AspNetCore.Mvc;
using Proyecto_JN_G7.Models;
using Proyecto_JN_G7.Services;

namespace Proyecto_JN_G7.Controllers
{
    public class PacienteController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _http;
        private readonly IUtilitarios _utilitarios;
        public PacienteController(IConfiguration configuration, IHttpClientFactory http, IUtilitarios utilitarios)
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
        public IActionResult Registro(Pacientes model)
        {
            using (var http = _http.CreateClient())
            {
                http.BaseAddress = new Uri(_configuration.GetSection("Start:ApiUrl").Value!);
                var resultado = http.PostAsJsonAsync("api/Paciente/Registro", model).Result;

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

        public IActionResult Index()
        {
            return View();
        }
    }
}
