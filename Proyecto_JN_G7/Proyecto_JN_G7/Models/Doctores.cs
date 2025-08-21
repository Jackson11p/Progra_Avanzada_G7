using Microsoft.AspNetCore.Mvc.Rendering;

namespace Proyecto_JN_G7.Models
{
    public class Doctores
    {
        public int? DoctorID { get; set; }

        public IEnumerable<SelectListItem>? Usuarios { get; set; } 
        public int UsuarioID { get; set; }

        public string? Especialidad { get; set; }

        public string? CedulaProfesional { get; set; }

    }
}
