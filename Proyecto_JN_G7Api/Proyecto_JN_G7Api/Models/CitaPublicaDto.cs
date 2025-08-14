using System.ComponentModel.DataAnnotations;

namespace Proyecto_JN_G7Api.Models
{
    public class CitaPublicaDto
    {
        [Required] public string Nombre { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required] public string Telefono { get; set; }

        [Required] public DateTime FechaHoraPreferida { get; set; }
        [Required] public string Especialidad { get; set; }

        public string? DoctorNombre { get; set; }
        public string? Mensaje { get; set; }
    }
}
