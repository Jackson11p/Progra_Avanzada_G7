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
public class HistorialMedicoController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly IUtilitarios _utilitarios;
    public HistorialMedicoController(IConfiguration configuration, IHostEnvironment environment, IUtilitarios utilitarios)
    {
        _configuration = configuration;
        _environment = environment;
        _utilitarios = utilitarios;
    }
    [HttpGet]
    [Route("ConsultarHistorialPorPaciente")]
    public IActionResult ConsultarHistorialPorPaciente(int pacienteId)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("Connection"));
        var resultado = connection.Query<HistorialMedico>("ConsultarHistorialPorPaciente", new { PacienteID = pacienteId }, commandType: CommandType.StoredProcedure);

        if (resultado != null && resultado.Any())
            return Ok(_utilitarios.RespuestaCorrecta(resultado));
        else
            return NotFound(_utilitarios.RespuestaIncorrecta("No se encontró historial para este paciente."));
    }

    [HttpPost]
    [Route("RegistrarHistorial")]
    public IActionResult RegistrarHistorial(HistorialMedico model)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("Connection"));
        var resultado = connection.Execute("RegistrarHistorial", new
        {
            model.PacienteID,
            model.DoctorID,
            model.DiagnosticoID
        }, commandType: CommandType.StoredProcedure);

        if (resultado > 0)
            return Ok(_utilitarios.RespuestaCorrecta(null));
        else
            return BadRequest(_utilitarios.RespuestaIncorrecta("No se pudo registrar el historial."));
    }
}