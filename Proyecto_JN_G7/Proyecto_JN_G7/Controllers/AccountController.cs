using Microsoft.AspNetCore.Mvc;
using Proyecto_JN_G7.Models;

namespace Proyecto_JN_G7.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Autenticacion autenticacion)
        {
            ViewBag.Mensaje = "No se pudo autenticar";
            return View();

            //return RedirectToAction("Principal","Home");
        }
    }
}
