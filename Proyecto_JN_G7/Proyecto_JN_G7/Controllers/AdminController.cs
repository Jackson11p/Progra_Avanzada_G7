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
            var tCitas = client.GetFromJsonAsync<List<CitaUnificada>>("api/Cita/Unificada");
            var tDoctores = client.GetFromJsonAsync<List<DoctorListItem>>("api/Doctor/ListaSimple");
            var tPacientes = client.GetFromJsonAsync<List<PacienteListItem>>("api/Paciente/ListaSimple");

            await Task.WhenAll(tCitas!, tDoctores!, tPacientes!);

            ViewBag.Doctores = tDoctores!.Result ?? new();
            ViewBag.Pacientes = tPacientes!.Result ?? new();

            return PartialView("Partials/_Citas", tCitas!.Result ?? new());
        }

        //consultar los docs
        public async Task<IActionResult> Doctores()
        {
            var client = _http.CreateClient("Api");
            var data = await client.GetFromJsonAsync<List<DoctorListItem>>("api/Doctor/ConsultarDoctor");
            return PartialView("Partials/_Doctores", data ?? new());
        }
        
        public IActionResult Pacientes() => PartialView("Partials/_Pacientes");
        public IActionResult HistorialMedico() => PartialView("Partials/_HistorialMedico");
        public IActionResult Facturas() => PartialView("Partials/_Facturas");
        public IActionResult Usuarios() => PartialView("Partials/_Usuarios");
    }
}
