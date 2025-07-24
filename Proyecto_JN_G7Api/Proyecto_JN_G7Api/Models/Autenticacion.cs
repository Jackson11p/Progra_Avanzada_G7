using Microsoft.AspNetCore.Mvc.Rendering;

namespace Proyecto_JN_G7Api.Models
{
    public class Autenticacion
    {
        public long UsuarioID { get; set; }
        public string? Cedula { get; set; }
        public string? NombreCompleto { get; set; }
        public string? CorreoElectronico { get; set; }
        public string? ContrasenaHash { get; set; }

        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
        public int RolID { get; set; }
        public string? NombreRol { get; set; }
    }
}
