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
    public class FacturasController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _http;
        private readonly IUtilitarios _utilitarios;

        public FacturasController(IConfiguration configuration, IHttpClientFactory http, IUtilitarios utilitarios)
        {
            _configuration = configuration;
            _http = http;
            _utilitarios = utilitarios;
        }

        public async Task<IActionResult> Index()
        {
            using var client = _http.CreateClient();
            client.BaseAddress = new Uri(_configuration.GetValue<string>("Start:ApiUrl"));

            var response = await client.GetAsync("api/Facturas");
            if (response.IsSuccessStatusCode)
            {
                var facturas = await response.Content.ReadFromJsonAsync<List<Factura>>();
                return View(facturas);
            }
            else
            {
                ViewBag.Mensaje = "No se pudieron cargar las facturas.";
                return View(new List<Factura>());
            }
        }

        public async Task<IActionResult> Create()
        {
            using var client = _http.CreateClient();
            client.BaseAddress = new Uri(_configuration.GetValue<string>("Start:ApiUrl"));

            // Obtener lista de pacientes para dropdown desde API
            var response = await client.GetAsync("api/Usuarios/Pacientes");
            if (response.IsSuccessStatusCode)
            {
                var pacientes = await response.Content.ReadFromJsonAsync<List<Autenticacion>>();
                ViewBag.Pacientes = pacientes.Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = p.UsuarioID.ToString(),
                    Text = p.NombreCompleto
                });
            }
            else
            {
                ViewBag.Pacientes = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Factura model)
        {
            if (ModelState.IsValid)
            {
                using var client = _http.CreateClient();
                client.BaseAddress = new Uri(_configuration.GetValue<string>("Start:ApiUrl"));

                var response = await client.PostAsJsonAsync("api/Facturas", model);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<RespuestaEstandar>();
                    ViewBag.Mensaje = error?.Mensaje ?? "Error al crear factura.";
                }
            }

            return View(model);
        }
    }
}
