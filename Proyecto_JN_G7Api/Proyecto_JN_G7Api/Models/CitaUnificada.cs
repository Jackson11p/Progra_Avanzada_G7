namespace Proyecto_JN_G7Api.Models
{
    public class CitaUnificada
    {
        public string Tipo { get; set; } = "";         // "Cita" | "Solicitud"
        public int? CitaID { get; set; }
        public long? SolicitudID { get; set; }        
        public int? PacienteID { get; set; }
        public string? PacienteNombre { get; set; }
        public int? DoctorID { get; set; }
        public string? DoctorNombre { get; set; }
        public DateTimeOffset FechaHora { get; set; }
        public string? Estado { get; set; }
        public string? Motivo { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public DateTimeOffset FechaRegistro { get; set; }
    }
}
