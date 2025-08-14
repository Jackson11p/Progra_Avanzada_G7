using Microsoft.AspNetCore.Mvc;
using Proyecto_JN_G7.Filters; 
namespace Proyecto_JN_G7.Controllers
{
    [AuthorizeAdmin]
    public class AdminController : Controller
    {
        public IActionResult Index() => View();

        // Endpoints para cargar parciales
        public IActionResult Doctores() => PartialView("Partials/_Doctores");
        public IActionResult Pacientes() => PartialView("Partials/_Pacientes");
        public IActionResult Citas() => PartialView("Partials/_Citas");
        public IActionResult HistorialMedico() => PartialView("Partials/_HistorialMedico");
        public IActionResult Facturas() => PartialView("Partials/_Facturas");
        public IActionResult Usuarios() => PartialView("Partials/_Usuarios");
    }
}
