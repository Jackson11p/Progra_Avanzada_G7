using Dapper;
using Microsoft.AspNetCore.Authorization;
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
    public class CitaController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly IUtilitarios _utilitarios;
        public CitaController(IConfiguration configuration, IHostEnvironment environment, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _environment = environment;
            _utilitarios = utilitarios;
        }

        //Registra un paciente
        [HttpPost]
        [Route("Registro")]
        public IActionResult Registro(Cita model)
        {
            var connectionString = _configuration.GetConnectionString("Connection");

            using (var context = new SqlConnection(connectionString))
            {
                var result = context.Execute("RegistrarCita",
                    new
                    {
                        model.PacienteID,
                        model.DoctorID,
                        model.FechaHora,
                        model.Estado,
                        model.MotivoConsulta,
                        model.FechaCreacion
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );

                return Ok("Registro exitoso");
            }
        }

        [HttpPost("RegistroPublico")]
        [AllowAnonymous]
        public IActionResult RegistroPublico([FromBody] CitaPublicaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(_utilitarios.RespuestaIncorrecta("Datos incompletos o inválidos."));

            try
            {
                using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
                conn.Execute("RegistrarCitaPublica", new
                {
                    dto.Nombre,
                    dto.Email,
                    dto.Telefono,
                    dto.FechaHoraPreferida,
                    dto.Especialidad,
                    DoctorNombre = dto.DoctorNombre,
                    Mensaje = dto.Mensaje
                }, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("LogsCitasPublicas_Fallback.txt",
                    $"[{DateTimeOffset.UtcNow:O}] Error al registrar solicitud: {ex.Message}{Environment.NewLine}");
                
            }

            return Ok(_utilitarios.RespuestaCorrecta(
                "¡Tu solicitud de cita fue enviada! Te contactaremos pronto para confirmar la disponibilidad."
            ));
        }
    }
}
