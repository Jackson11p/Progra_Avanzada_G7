namespace Proyecto_JN_G7Api.Models
{
    public class Cita
    {
        public int CitaID { get; set; }
        public int PacienteID { get; set; }

        public int DoctorID { get; set; }

        public DateTime FechaHora { get; set; }

        public string? Estado { get; set; }

        public string? MotivoConsulta { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}