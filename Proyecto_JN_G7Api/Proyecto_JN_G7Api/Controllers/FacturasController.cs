using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Proyecto_JN_G7Api.Models;
using Proyecto_JN_G7Api.Services;

[Route("api/[controller]")]
[ApiController]
public class FacturasController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly IUtilitarios _utilitarios;
    public FacturasController(IConfiguration configuration, IHostEnvironment environment, IUtilitarios utilitarios)
    {
        _configuration = configuration;
        _environment = environment;
        _utilitarios = utilitarios;
    }

    [HttpGet]
    [Route("ConsultarFactura")]
    public IActionResult ConsultarFactura(int facturaId)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("Connection"));
        var resultado = connection.Query<Factura>("ConsultarFactura", new { FacturaID = facturaId }, commandType: CommandType.StoredProcedure);

        if (resultado != null && resultado.Any())
            return Ok(_utilitarios.RespuestaCorrecta(resultado));
        else
            return NotFound(_utilitarios.RespuestaIncorrecta("Factura no encontrada."));
    }

    [HttpPost]
    [Route("CrearFactura")]
    public IActionResult CrearFactura(Factura model)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("Connection"));
        var resultado = connection.Execute("CrearFactura", new
        {
            model.CitaID,
            model.Total,
            model.EstadoPago
        }, commandType: CommandType.StoredProcedure);

        if (resultado > 0)
            return Ok(_utilitarios.RespuestaCorrecta(null));
        else
            return BadRequest(_utilitarios.RespuestaIncorrecta("Error al crear la factura."));
    }
}
