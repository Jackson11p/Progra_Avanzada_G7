using Microsoft.AspNetCore.Mvc;
using Proyecto_JN_G7.Filters;
using Proyecto_JN_G7.Models;
namespace Proyecto_JN_G7.Controllers
{
    [AuthorizeAdmin]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _cfg;
        public AdminController(IConfiguration cfg, IHttpClientFactory http)
        {
            _cfg = cfg; _http = http;
        }
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

        //consultar las facturas
        public async Task<IActionResult> Facturas()
        {
            var client = _http.CreateClient("Api");
            var data = await client.GetFromJsonAsync<List<Factura>>("api/Facturas/ConsultarFacturas");
            return PartialView("Partials/_Facturas", data ?? new());
        }

        public async Task<IActionResult> HistorialMedico()
        {
            var api = _cfg["ApiBaseUrl"] ?? "/";
            var cli = _http.CreateClient();

            var lista = new List<PacienteListItem>();
            try
            {
                var resp = await cli.GetAsync($"{api}api/Paciente/ListaSimple");
                if (resp.IsSuccessStatusCode)
                    lista = await resp.Content.ReadFromJsonAsync<List<PacienteListItem>>() ?? new();
            }
            catch { }

            ViewBag.Pacientes = lista;
            return PartialView("Partials/_HistorialMedico");
        }

        public IActionResult Pacientes() => PartialView("Partials/_Pacientes");    
        public IActionResult Usuarios() => PartialView("Partials/_Usuarios");
    }
}
