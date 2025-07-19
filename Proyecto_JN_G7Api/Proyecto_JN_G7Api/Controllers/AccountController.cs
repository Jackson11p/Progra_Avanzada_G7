using System;
using System.Reflection;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Proyecto_JN_G7Api.Models;
using Proyecto_JN_G7Api.Services;

namespace Proyecto_JN_G7Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly IUtilitarios _utilitarios;
        public AccountController(IConfiguration configuration, IHostEnvironment environment, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _environment = environment;
            _utilitarios = utilitarios;
        }

        [HttpPost]
        [Route("Registro")]
        public IActionResult Registro(Autenticacion model)
        {
            var connectionString = _configuration.GetConnectionString("Connection");

            using (var context = new SqlConnection(connectionString))
            {
                var result = context.Execute("RegistrarUsuario",
                    new
                    {
                        model.Cedula,
                        model.NombreCompleto,
                        model.CorreoElectronico,
                        model.ContrasenaHash
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );

                return Ok("Registro exitoso");
            }
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login(Autenticacion model)
        {
            var connectionString = _configuration.GetConnectionString("Connection");

            using (var context = new SqlConnection(connectionString))
            {
                var usuario = context.QueryFirstOrDefault<Autenticacion>(
                    "IniciarSesion",
                    new
                    {
                        model.CorreoElectronico,
                        model.ContrasenaHash
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );

                if (usuario != null)
                {
                    return Ok(usuario);
                }
                else
                {
                    return NotFound(new RespuestaEstandar { Mensaje = "Usuario no encontrado o credenciales incorrectas." });
                }
            }
        }

        [HttpPost]
        [Route("RecuperarAcceso")]
        public IActionResult RecuperarAcceso(Autenticacion model)
        {
            using (var context = new SqlConnection(_configuration.GetSection("ConnectionStrings:Connection").Value))
            {
                var resultado = context.QueryFirstOrDefault<Autenticacion>("ValidarCorreo",
                    new { model.CorreoElectronico });

                if (resultado != null)
                {
                    var ContrasennaNotificar = _utilitarios.GenerarContrasenna(10);
                    var ContrasenaHash = _utilitarios.Encrypt(ContrasennaNotificar);

                    var resultadoActualizacion = context.Execute("ActualizarContrasenna",
                    new
                    {
                        resultado.UsuarioID,
                        ContrasenaHash
                    },
                    commandType: System.Data.CommandType.StoredProcedure);


                    if (resultadoActualizacion > 0)
                    {
                        var ruta = Path.Combine(_environment.ContentRootPath, "Correos.html");
                        var html = System.IO.File.ReadAllText(ruta, UTF8Encoding.UTF8);

                        html = html.Replace("@@Usuario", resultado.NombreCompleto);
                        html = html.Replace("@@Contrasenna", ContrasennaNotificar);

                        _utilitarios.EnviarCorreo(resultado.CorreoElectronico!, "Recuperación de Acceso", html);
                        return Ok(_utilitarios.RespuestaCorrecta(null));
                    }
                }

                return BadRequest(_utilitarios.RespuestaIncorrecta("Su información no fue validada"));
            }
        }
    }
}
