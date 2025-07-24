using Microsoft.AspNetCore.Mvc;

namespace Proyecto_JN_G7Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        [HttpGet("error")]
        public IActionResult ThrowTestError()
        {
            // Esto provocará una excepción no capturada
            throw new InvalidOperationException("Error de prueba desde Swagger");
        }
    }
}
