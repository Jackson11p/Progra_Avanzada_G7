using System.ComponentModel.DataAnnotations;

namespace Proyecto_JN_G7Api.Models
{
    public class AtenderSolicitud
    {
        [Required] public int PacienteID { get; set; }
        [Required] public int DoctorID { get; set; }
        [Required] public DateTime FechaHora { get; set; }

        [Required, StringLength(20)]
        public string Estado { get; set; } = "Pendiente";

        public string? MotivoConsulta { get; set; }
    }
}
