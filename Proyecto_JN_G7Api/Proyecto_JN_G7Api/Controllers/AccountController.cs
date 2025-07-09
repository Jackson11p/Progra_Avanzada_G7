using System;
using System.Reflection;
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
    }
}
