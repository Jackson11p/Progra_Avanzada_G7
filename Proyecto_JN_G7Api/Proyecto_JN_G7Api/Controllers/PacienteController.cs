using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Proyecto_JN_G7Api.Models;
using Proyecto_JN_G7Api.Services;
using System;
using System.Data;
using System.Reflection;
using System.Text;

namespace Proyecto_JN_G7Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacienteController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly IUtilitarios _utilitarios;
        public PacienteController(IConfiguration configuration, IHostEnvironment environment, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _environment = environment;
            _utilitarios = utilitarios;
        }

        [HttpGet("ListaSimple")]
        public IActionResult ListaSimple()
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var data = conn.Query<PacienteListItem>("Paciente_ListaSimple",
                        commandType: CommandType.StoredProcedure).ToList();
            return Ok(data);
        }

        [HttpGet("Buscar")]
        public IActionResult Buscar([FromQuery] string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Debe enviar 'email'.");

            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var item = conn.QueryFirstOrDefault<PacienteListItem>(
                "Paciente_Buscar",
                new { Email = email },
                commandType: CommandType.StoredProcedure
            );

            return item is null ? NotFound() : Ok(item);
        }

        public class PacienteCrearReq
        {
            public string Cedula { get; set; } = "";
            public string NombreCompleto { get; set; } = "";
            public string CorreoElectronico { get; set; } = "";
            public string? ContrasenaHash { get; set; }
            public string? Telefono { get; set; }
            public DateTime? FechaNacimiento { get; set; }
            public string? Genero { get; set; }
            public string? Direccion { get; set; }
        }

        [HttpPost]
        public IActionResult Crear([FromBody] PacienteCrearReq body)
        {
            if (string.IsNullOrWhiteSpace(body.Cedula) ||
                string.IsNullOrWhiteSpace(body.NombreCompleto) ||
                string.IsNullOrWhiteSpace(body.CorreoElectronico))
            {
                return BadRequest(_utilitarios.RespuestaIncorrecta("Cédula, nombre y correo son obligatorios."));
            }

            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var id = conn.QuerySingle<int>("Paciente_Crear", new
            {
                body.Cedula,
                body.NombreCompleto,
                body.CorreoElectronico,
                body.ContrasenaHash,
                body.Telefono,
                body.FechaNacimiento,
                body.Genero,
                body.Direccion
            }, commandType: CommandType.StoredProcedure);

            return Ok(new { pacienteID = id });
        }

    }
}
