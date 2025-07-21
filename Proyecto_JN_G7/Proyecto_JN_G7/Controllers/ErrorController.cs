using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Proyecto_JN_G7.Models;
using static System.Net.WebRequestMethods;

namespace Proyecto_JN_G7Api.Controllers
{
    public class ClientErrorDto
    {
        public long? UsuarioID { get; set; }
        public string? Origen { get; set; }
        public string? TipoError { get; set; }
        public string? Mensaje { get; set; }
        public string? StackTrace { get; set; }
        public string? RequestId { get; set; }
        public string? IpCliente { get; set; }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<ErrorController> _logger;
        private readonly IConfiguration _configuration;

        public ErrorController(IHttpClientFactory httpFactory, ILogger<ErrorController> logger, IConfiguration configuration)
        {
            _http = httpFactory;
            _logger = logger;
            _configuration = configuration;
        }

        [Route("Error")]
        public async Task<IActionResult> Error()
        {
            var feat = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var ex = feat?.Error;
            var path = feat?.Path;
            var reqId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            //log local
            _logger.LogError(ex, "Error en {Path} (RequestId: {Id})", path, reqId);

            //reportar al API de logs (fire-and-forget)
            try
            {
                var client = _http.CreateClient();
                client.BaseAddress = new Uri(_configuration.GetSection("Start:ApiUrl").Value!); 

                var dto = new ClientErrorDto
                {
                    UsuarioID = null,
                    Origen = path ?? "",
                    TipoError = ex?.GetType().Name ?? "",
                    Mensaje = ex?.Message ?? "",
                    StackTrace = ex?.StackTrace ?? "",
                    RequestId = reqId,
                    IpCliente = ip
                };
                // Ajusta la URL base en tu HttpClient si lo necesitas
                _ = client.PostAsJsonAsync("/api/Error/CapturarError", dto);
            }
            catch (Exception logEx)
            {
                _logger.LogWarning(logEx, "Falló reporte de error al API");
            }

            // 3) Devolver la vista Error
            return View("Error", new ErrorView { RequestId = reqId });
        }
    }
}
