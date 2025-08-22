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
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;

namespace Proyecto_JN_G7Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitaController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly IWebHostEnvironment _env;
        private readonly IUtilitarios _utilitarios;
        public CitaController(IConfiguration configuration, IHostEnvironment environment, IWebHostEnvironment env, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _environment = environment;
            _utilitarios = utilitarios;
            _env = env;
        }

        private string GetWebRootPath()
        {
            var wwwroot = _env.WebRootPath;
            if (string.IsNullOrEmpty(wwwroot))
                wwwroot = Path.Combine(_env.ContentRootPath, "wwwroot");
            Directory.CreateDirectory(wwwroot);
            return wwwroot;
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

        [HttpGet("Historial/{pacienteId:int}")]
        public IActionResult HistorialPorPaciente(int pacienteId)
        {
            if (pacienteId <= 0)
                return BadRequest(_utilitarios.RespuestaIncorrecta("Paciente inválido."));

            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            try
            {
                var lista = conn.Query<CitaHistorialItem>(
                    "Cita_HistorialPorPaciente",
                    new { PacienteID = pacienteId },
                    commandType: CommandType.StoredProcedure
                ).AsList();

         
                return Ok(lista);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, _utilitarios.RespuestaIncorrecta(ex.Message));
            }
        }


        [HttpGet("{citaId:int}/Adjuntos")]
        public IActionResult ListarAdjuntos(int citaId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var lista = conn.Query<CitaAdjuntoItem>("CitaAdjunto_ListarPorCita",
                new { CitaID = citaId }, commandType: CommandType.StoredProcedure).ToList();

            return Ok(_utilitarios.RespuestaCorrecta(lista));
        }


        [HttpPost("{citaId:int}/Adjuntos")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> SubirAdjunto(int citaId, IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(_utilitarios.RespuestaIncorrecta("Debe adjuntar un archivo."));

            var wwwroot = GetWebRootPath();
            var folder = Path.Combine(wwwroot, "uploads", "citas", citaId.ToString());
            Directory.CreateDirectory(folder);

            var safeName = SanitizeFileName(file.FileName);
            var uniqueName = $"{Guid.NewGuid():N}_{safeName}";
            var fullPath = Path.Combine(folder, uniqueName);

            await using (var fs = System.IO.File.Create(fullPath))
                await file.CopyToAsync(fs);

            var relPath = Path.Combine("uploads", "citas", citaId.ToString(), uniqueName).Replace('\\', '/');

            int adjuntoId;
            using (var conn = new SqlConnection(_configuration.GetConnectionString("Connection")))
            {
                adjuntoId = conn.QuerySingle<int>("CitaAdjunto_Insertar", new
                {
                    CitaID = citaId,
                    NombreArchivo = safeName,
                    RutaRelativa = relPath,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    SizeBytes = (long?)file.Length,
                    SubidoPorUsuarioID = (int?)null  
                }, commandType: CommandType.StoredProcedure);
            }

            var url = $"{Request.Scheme}://{Request.Host}/{relPath}";
            return Ok(_utilitarios.RespuestaCorrecta(new
            {
                AdjuntoID = adjuntoId,
                NombreArchivo = safeName,
                RutaRelativa = relPath,
                Url = url,
                ContentType = file.ContentType ?? "application/octet-stream",
                SizeBytes = file.Length
            }));
        }


        [HttpGet("Adjuntos/{adjuntoId:int}/Descargar")]
        public IActionResult DescargarAdjunto(int adjuntoId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var item = conn.QueryFirstOrDefault<dynamic>(
                "CitaAdjunto_Obtener",
                new { AdjuntoID = adjuntoId },
                commandType: CommandType.StoredProcedure
            );

            if (item == null)
                return NotFound(_utilitarios.RespuestaIncorrecta("Adjunto inexistente."));

            string rutaRel = (string?)item.RutaRelativa ?? "";
            string fullPath = Path.Combine(GetWebRootPath(), rutaRel.Replace('/', Path.DirectorySeparatorChar));

            if (!System.IO.File.Exists(fullPath))
                return NotFound(_utilitarios.RespuestaIncorrecta("Archivo no encontrado en servidor."));

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            string contentType = (string?)item.ContentType ?? "application/octet-stream";
            string nombre = (string?)item.NombreArchivo ?? Path.GetFileName(fullPath);

            return File(stream, contentType, fileDownloadName: nombre);
        }

        [HttpDelete("Adjuntos/{adjuntoId:int}")]
        public IActionResult EliminarAdjunto(int adjuntoId)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var afectados = conn.QuerySingle<int>("CitaAdjunto_Eliminar",
                new { AdjuntoID = adjuntoId }, commandType: CommandType.StoredProcedure);

            if (afectados == 0) return NotFound(_utilitarios.RespuestaIncorrecta("Adjunto no existe."));
            return Ok(_utilitarios.RespuestaCorrecta(null));
        }

        private static string SanitizeFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            var safe = Regex.Replace(fileName, @"[^A-Za-z0-9\.\-_]+", "_");
            return string.IsNullOrWhiteSpace(safe) ? "archivo" : safe;
        }
    }
}
