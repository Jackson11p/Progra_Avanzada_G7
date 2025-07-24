using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Proyecto_JN_G7Api.Models;
using Proyecto_JN_G7Api.Services;

namespace Proyecto_JN_G7Api.Controllers
{
    //[ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    public class ContactoController : Controller
    {
        private readonly IUtilitarios _utilitarios;
        private readonly IConfiguration _configuration;

        public ContactoController(
            IUtilitarios utilitarios,
            IConfiguration configuration)
        {
            _utilitarios = utilitarios;
            _configuration = configuration;
        }

        [HttpPost("Enviar")]
        public IActionResult Enviar([FromBody] Contacto dto)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { errores });
            }

            var destinatario = _configuration["Contact:Destinatario"]!;
            var asuntoNegocio = $"Contacto recibido: {dto.Asunto}";
            var cuerpoNegocio = $@"
                <p><strong>Nombre:</strong> {dto.Nombre}</p>
                <p><strong>Email:</strong> {dto.Email}</p>
                <p><strong>Asunto:</strong> {dto.Asunto}</p>
                <p><strong>Mensaje:</strong><br/>{dto.Mensaje}</p>";

            _utilitarios.EnviarCorreo(destinatario, asuntoNegocio, cuerpoNegocio);

            var asuntoUsuario = "Gracias por contactarnos";
            var cuerpoUsuario = $@"
                <p>Hola <strong>{dto.Nombre}</strong>,</p>
                <p>Gracias por escribirnos. Hemos recibido tu mensaje y te responderemos a la brevedad.</p>
                <p>— El equipo de HealthyLife</p>";

            _utilitarios.EnviarCorreo(dto.Email, asuntoUsuario, cuerpoUsuario);

            return Ok();
        }
    }
}
