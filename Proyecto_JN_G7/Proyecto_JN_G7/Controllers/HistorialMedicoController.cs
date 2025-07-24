using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Proyecto_JN_G7.Models;
using Proyecto_JN_G7.Services;
using System.Collections.Generic;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Proyecto_JN_G7.Controllers
{
    public class HistorialMedicoController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _http;
        private readonly IUtilitarios _utilitarios;

        public HistorialMedicoController(IConfiguration configuration, IHttpClientFactory http, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _http = http;
            _utilitarios = utilitarios;
        }
        //public async Task<IActionResult> Index()
        //{
        //    var usuario = _utilitarios.ObtenerUsuario(HttpContext);

        //    if (usuario == null || usuario.RolID != 3) // Rol 3 = paciente
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }

        //    using var client = _http.CreateClient();
        //    client.BaseAddress = new Uri(_configuration.GetValue<string>("Start:ApiUrl"));

        //    var response = await client.GetAsync($"api/HistorialMedico/paciente/{usuario.UsuarioID}");
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var historial = await response.Content.ReadFromJsonAsync<List<HistorialMedico>>();
        //        return View(historial);
        //    }

        //    ViewBag.Mensaje = "No se pudo cargar el historial médico.";
        //    return View(new List<HistorialMedico>());
        //}
    }     
}