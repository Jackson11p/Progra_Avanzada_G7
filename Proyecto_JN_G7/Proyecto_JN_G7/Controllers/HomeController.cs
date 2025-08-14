using Microsoft.AspNetCore.Mvc;
using Proyecto_JN_G7.Models;
using System.Diagnostics;
using static System.Net.WebRequestMethods;

namespace Proyecto_JN_G7.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _http;
        public HomeController(IHttpClientFactory http, ILogger<HomeController> logger)
        {
            _http = http;
            _logger = logger;
        }

        [Route("Home/TestError")]
        public IActionResult TestError()
        {
            throw new InvalidOperationException("Error de prueba desde Front-MVC");
        }


        public async Task<IActionResult> Index()
        {
            var client = _http.CreateClient("Api");
            var doctores = await client.GetFromJsonAsync<List<DoctorListItem>>("api/Doctor/ListaSimple")
                           ?? new List<DoctorListItem>();

            ViewBag.Doctores = doctores;
            return View();
        }

        public IActionResult Main()
        {
            return View();
        }

    }
}
