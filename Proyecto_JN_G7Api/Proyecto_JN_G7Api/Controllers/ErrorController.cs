using System;
using System.IO;
using System.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Dapper;
using Proyecto_JN_G7Api.Services;

[Route("api/[controller]")]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUtilitarios _utilitarios;
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(
        IConfiguration configuration,
        IUtilitarios utilitarios,
        ILogger<ErrorController> logger)
    {
        _configuration = configuration;
        _utilitarios = utilitarios;
        _logger = logger;
    }

    //acción para el middleware dentro del api
    [HttpGet("CapturarError")]
    public IActionResult CapturarErrorGet()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;
        var ex = feature.Error;
        var origen = HttpContext.Request.Path.ToString();
        var tipoError = ex.GetType().Name;
        var mensaje = ex.Message;
        var stack = ex.StackTrace;
        var requestId = HttpContext.TraceIdentifier;
        var ipCliente = HttpContext.Connection.RemoteIpAddress?.ToString();
        long usuarioId = 0; // pendiente extraer del JWT

        using var conn = new SqlConnection(_configuration["ConnectionStrings:Connection"]);
        conn.Execute("RegistrarError", new
        {
            UsuarioID = usuarioId,
            Origen = origen,
            TipoError = tipoError,
            Mensaje = mensaje,
            StackTrace = stack,
            RequestId = requestId,
            IpCliente = ipCliente
        }, commandType: CommandType.StoredProcedure);

        return StatusCode(500, _utilitarios.RespuestaIncorrecta("Se presentó un error interno"));
    }

    //acción para recibir logs desde el front (POST)
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

    [HttpPost("CapturarError")]
    public IActionResult CapturarErrorPost([FromBody] ClientErrorDto dto)
    {
        if (dto == null)
            return BadRequest("json inválido");

        //mensaje del front en formato json viene en el dto
        using var conn = new SqlConnection(_configuration["ConnectionStrings:Connection"]);
        conn.Execute("RegistrarError", new
        {
            UsuarioID = dto.UsuarioID,
            Origen = dto.Origen,
            TipoError = dto.TipoError,
            Mensaje = dto.Mensaje,
            StackTrace = dto.StackTrace,
            RequestId = dto.RequestId,
            IpCliente = dto.IpCliente
        }, commandType: CommandType.StoredProcedure);

        return Ok();
    }
}
