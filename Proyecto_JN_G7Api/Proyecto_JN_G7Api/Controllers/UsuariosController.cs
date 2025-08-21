using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Proyecto_JN_G7Api.Models;
using Proyecto_JN_G7Api.Services;

namespace Proyecto_JN_G7Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly IUtilitarios _utilitarios;

        public UsuariosController(IConfiguration configuration, IHostEnvironment environment, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _environment = environment;
            _utilitarios = utilitarios;
        }

        [HttpGet("ConsultarUsuario")]
        public IActionResult ConsultarUsuario(long UsuarioID)
        {
            using var context = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var resultado = context.QueryFirstOrDefault<Autenticacion>(
                "ConsultarUsuario",
                new { UsuarioID },
                commandType: CommandType.StoredProcedure
            );

            return resultado != null
                ? Ok(_utilitarios.RespuestaCorrecta(resultado))
                : BadRequest(_utilitarios.RespuestaIncorrecta("Su información no fue validada"));
        }

        [HttpPut("ActualizarUsuario")]
        public IActionResult ActualizarUsuario(Autenticacion model)
        {
            using var context = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var filas = context.Execute(
                "ActualizarUsuario",
                new
                {
                    model.UsuarioID,
                    model.Cedula,
                    model.NombreCompleto,
                    model.CorreoElectronico
                },
                commandType: CommandType.StoredProcedure
            );

            return filas > 0
                ? Ok(_utilitarios.RespuestaCorrecta(null))
                : BadRequest(_utilitarios.RespuestaIncorrecta("Su información no fue actualizada"));
        }

        [HttpPut("ActualizarContrasenna")]
        public IActionResult ActualizarContrasenna(Autenticacion model)
        {
            using var context = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var filas = context.Execute(
                "ActualizarContrasenna",
                new
                {
                    model.UsuarioID,
                    model.ContrasenaHash
                },
                commandType: CommandType.StoredProcedure
            );

            return filas > 0
                ? Ok(_utilitarios.RespuestaCorrecta(null))
                : BadRequest(_utilitarios.RespuestaIncorrecta("Su información no fue actualizada"));
        }

        [HttpGet("ConsultarUsuariosDropdown")]
        public IActionResult ConsultarUsuariosDropdown()
        {
            using var context = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var resultado = context.Query<UsuarioDropdown>(
                "ConsultarUsuariosDropdown",
                commandType: CommandType.StoredProcedure
            );
            return Ok(_utilitarios.RespuestaCorrecta(resultado));
        }

        [HttpGet("Roles")]
        public IActionResult Roles()
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var items = conn.Query<RolItem>("Roles_Listar", commandType: CommandType.StoredProcedure);
            return Ok(_utilitarios.RespuestaCorrecta(items));
        }

        [HttpGet]
        public IActionResult Listar()
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var items = conn.Query<UsuarioListItem>("Usuario_Listar", commandType: CommandType.StoredProcedure);
            return Ok(_utilitarios.RespuestaCorrecta(items));
        }

        [HttpGet("{id:int}")]
        public IActionResult Obtener(int id)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var item = conn.QueryFirstOrDefault<UsuarioDetalle>(
                "Usuario_Obtener",
                new { UsuarioID = id },
                commandType: CommandType.StoredProcedure
            );

            return item is null
                ? NotFound(_utilitarios.RespuestaIncorrecta("Usuario no encontrado."))
                : Ok(_utilitarios.RespuestaCorrecta(item));
        }

        [HttpPost]
        public IActionResult Crear([FromBody] UsuarioCrear body)
        {
            if (string.IsNullOrWhiteSpace(body.Cedula) ||
                string.IsNullOrWhiteSpace(body.NombreCompleto) ||
                string.IsNullOrWhiteSpace(body.CorreoElectronico) ||
                string.IsNullOrWhiteSpace(body.Contrasena))
            {
                return BadRequest(_utilitarios.RespuestaIncorrecta(
                    "Cédula, nombre, correo y contraseña son obligatorios."));
            }

            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var newId = conn.QuerySingle<int>(
                "Usuario_Crear",
                new
                {
                    body.Cedula,
                    body.NombreCompleto,
                    body.CorreoElectronico,
                    ContrasenaHash = _utilitarios.Encrypt(body.Contrasena),
                    RolID = body.RolID <= 0 ? 1 : body.RolID // 1 = Usuario
                },
                commandType: CommandType.StoredProcedure
            );

            return Ok(_utilitarios.RespuestaCorrecta(new { UsuarioID = newId }));
        }

        [HttpPut("{id:int}")]
        public IActionResult Actualizar(int id, [FromBody] UsuarioActualizar body)
        {
            if (string.IsNullOrWhiteSpace(body.Cedula) ||
                string.IsNullOrWhiteSpace(body.NombreCompleto) ||
                string.IsNullOrWhiteSpace(body.CorreoElectronico))
            {
                return BadRequest(_utilitarios.RespuestaIncorrecta("Cédula, nombre y correo son obligatorios."));
            }

            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            conn.Execute(
                "Usuario_Actualizar",
                new
                {
                    UsuarioID = id,
                    body.Cedula,
                    body.NombreCompleto,
                    body.CorreoElectronico,
                    body.RolID,
                    body.Activo
                },
                commandType: CommandType.StoredProcedure
            );

            return Ok(_utilitarios.RespuestaCorrecta(null));
        }

        [HttpPut("{id:int}/estado/toggle")]
        public IActionResult ToggleEstado(int id)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            conn.Execute(
                "UPDATE Usuarios SET Activo = CASE WHEN Activo = 1 THEN 0 ELSE 1 END WHERE UsuarioID = @UsuarioID",
                new { UsuarioID = id }
            );
            return Ok(_utilitarios.RespuestaCorrecta(null));
        }

        [HttpPut("{id:int}/rol")]
        public IActionResult CambiarRol(int id, [FromBody] CambiarRol body)
        {
            if (body.RolID <= 0)
                return BadRequest(_utilitarios.RespuestaIncorrecta("Rol inválido."));

            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            conn.Execute(
                "UPDATE Usuarios SET RolID = @RolID WHERE UsuarioID = @UsuarioID",
                new { UsuarioID = id, body.RolID }
            );
            return Ok(_utilitarios.RespuestaCorrecta(null));
        }

        [HttpPut("{id:int}/password")]
        public IActionResult ResetPassword(int id, [FromBody] ResetPassword body)
        {
            if (string.IsNullOrWhiteSpace(body.Contrasena))
                return BadRequest(_utilitarios.RespuestaIncorrecta("Contraseña obligatoria."));

            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            conn.Execute(
                "Usuario_ResetPassword",
                new { UsuarioID = id, ContrasenaHash = _utilitarios.Encrypt(body.Contrasena) },
                commandType: CommandType.StoredProcedure
            );

            return Ok(_utilitarios.RespuestaCorrecta(null));
        }
    }
}
