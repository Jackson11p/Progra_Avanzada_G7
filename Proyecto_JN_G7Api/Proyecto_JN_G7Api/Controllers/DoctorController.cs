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
    public class DoctorController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly IUtilitarios _utilitarios;
        public DoctorController(IConfiguration configuration, IHostEnvironment environment, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _environment = environment;
            _utilitarios = utilitarios;
        }

        [HttpGet("ListaSimple")]
        public IActionResult ListaSimple()
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("Connection"));
            var sql = @"
                SELECT d.DoctorID,
                       u.NombreCompleto AS Nombre,
                       d.Especialidad
                FROM Doctores d
                INNER JOIN Usuarios u ON u.UsuarioID = d.UsuarioID
                WHERE u.Activo = 1
                ORDER BY u.NombreCompleto";
            var data = conn.Query<DoctorListItem>(sql).ToList();
            return Ok(data);
        }

        //Registra un doctor
        [HttpPost]
        [Route("Registro")]
        public IActionResult Registro(Doctores model)
        {
            var connectionString = _configuration.GetConnectionString("Connection");

            using (var context = new SqlConnection(connectionString))
            {
                var result = context.Execute("RegistrarDcotor",
                    new
                    {
                        model.UsuarioID,
                        model.Especialidad,
                        model.CedulaProfesional
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );

                return Ok("Registro exitoso");
            }
        }
    }
}
