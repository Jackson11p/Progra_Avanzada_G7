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



        
    }
}
