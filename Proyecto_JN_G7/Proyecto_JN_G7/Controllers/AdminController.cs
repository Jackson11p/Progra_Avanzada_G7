using Microsoft.AspNetCore.Mvc;
using Proyecto_JN_G7.Filters;
using Proyecto_JN_G7.Models;
namespace Proyecto_JN_G7.Controllers
{
    [AuthorizeAdmin]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _http;
        public AdminController(IHttpClientFactory http) => _http = http;
        public IActionResult Index() => View();

        public async Task<IActionResult> Citas()
        {
            var client = _http.CreateClient("Api");
            var data = await client.GetFromJsonAsync<List<CitaUnificada>>("api/Cita/Unificada");
            return PartialView("Partials/_Citas", data ?? new());
        }

        public IActionResult Doctores() => PartialView("Partials/_Doctores");
        public IActionResult Pacientes() => PartialView("Partials/_Pacientes");
        public IActionResult HistorialMedico() => PartialView("Partials/_HistorialMedico");
        public IActionResult Facturas() => PartialView("Partials/_Facturas");
        public IActionResult Usuarios() => PartialView("Partials/_Usuarios");
    }
}
