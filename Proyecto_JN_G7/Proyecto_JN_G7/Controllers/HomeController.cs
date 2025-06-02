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
