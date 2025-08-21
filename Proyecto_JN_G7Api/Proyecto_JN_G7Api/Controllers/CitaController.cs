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

        [HttpGet("Unificada")]
        public IActionResult Unificada()
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var data = conn.Query<CitaUnificada>(
                "Cita_Listar_Unificada",
                commandType: CommandType.StoredProcedure);
            return Ok(data);
        }


        public record CitaCreateReq(
            int PacienteID,
            int DoctorID,
            DateTime FechaHora,
            string Estado,
            string? MotivoConsulta
        );

        [HttpPost]
        public IActionResult Crear([FromBody] CitaCreateReq dto)
        {
            if (dto.PacienteID <= 0 || dto.DoctorID <= 0 || dto.FechaHora == default)
                return BadRequest(_utilitarios.RespuestaIncorrecta("Paciente, doctor y fecha son obligatorios."));

            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            try
            {
                var id = conn.QuerySingle<int>("Cita_Crear", new
                {
                    dto.PacienteID,
                    dto.DoctorID,
                    dto.FechaHora,
                    dto.Estado,
                    dto.MotivoConsulta
                }, commandType: CommandType.StoredProcedure);

                return Ok(new { CitaID = id });
            }
            catch (SqlException ex)
            {
                return BadRequest(_utilitarios.RespuestaIncorrecta(ex.Message));
            }
        }

        //Obtiene las citas de un usuario
        [HttpGet]
        [Route("ConsultarCitasUsuario")]
        public IActionResult ConsultarCitasUsuario(long UsuarioID)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:Connection").Value))
            {
                var resultado = context.Query<CitasPorUsuarioUnificada>("CitasPorUsuarioUnificada",
                    new
                    {
                        UsuarioID
                    },
                commandType: CommandType.StoredProcedure);


                if (resultado != null && resultado.Any())
                {
                    return Ok(_utilitarios.RespuestaCorrecta(resultado));
                }
                else
                {
                    return BadRequest(_utilitarios.RespuestaIncorrecta("No se encontraron citas"));
                }
            }
        }
           

        [HttpPut("{id:int}")]
        public IActionResult Actualizar(int id, [FromBody] Models.Cita model)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var afectados = conn.QuerySingle<int>("Cita_Actualizar", new
            {
                CitaID = id,
                model.PacienteID,
                model.DoctorID,
                model.FechaHora,
                model.Estado,
                model.MotivoConsulta
            }, commandType: CommandType.StoredProcedure);

            if (afectados == 0) return NotFound("No existe la cita.");
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public IActionResult Eliminar(int id)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var afectados = conn.QuerySingle<int>("Cita_Eliminar", new { CitaID = id }, commandType: CommandType.StoredProcedure);
            if (afectados == 0) return NotFound("No existe la cita.");
            return NoContent();
        }

        [HttpPost("Solicitudes/{solicitudId:long}/Atender")]
        public IActionResult AtenderSolicitud(long solicitudId, [FromBody] AtenderSolicitud body)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var newId = conn.QuerySingle<int>("CitaPublica_Atender", new
            {
                SolicitudID = solicitudId,
                body.PacienteID,
                body.DoctorID,
                body.FechaHora,
                body.Estado,
                body.MotivoConsulta
            }, commandType: CommandType.StoredProcedure);

            return Ok(new { CitaID = newId, Mensaje = "Solicitud atendida y convertida a cita." });
        }

        [HttpDelete("Solicitudes/{solicitudId:long}")]
        public IActionResult EliminarSolicitud(long solicitudId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var afectados = conn.QuerySingle<int>("CitaPublica_Eliminar", new { SolicitudID = solicitudId }, commandType: CommandType.StoredProcedure);
            if (afectados == 0) return NotFound("No existe la solicitud.");
            return NoContent();
        }
    }
}
