using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Proyecto_JN_G7.Models;

namespace Proyecto_JN_G7.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [Route("Home/TestError")]
        public IActionResult TestError()
        {
            throw new InvalidOperationException("Error de prueba desde Front-MVC");
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Main()
        {
            return View();
        }

    }
}
