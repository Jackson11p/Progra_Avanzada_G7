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

        //Registra un paciente
        [HttpPost]
        [Route("Registro")]
        public IActionResult Registro(Pacientes model)
        {
            var connectionString = _configuration.GetConnectionString("Connection");

            using (var context = new SqlConnection(connectionString))
            {
                var result = context.Execute("RegistrarPaciente",
                    new
                    {
                        model.NombreCompleto,
                        model.FechaNacimiento,
                        model.Genero,
                        model.Direccion,
                        model.Telefono,
                        model.CorreoElectronico
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );

                return Ok("Registro exitoso");
            }
        }
    }
}
