namespace Proyecto_JN_G7Api.Models
{
    public class CitaHistorialItem
    {
        public int CitaID { get; set; }
        public int PacienteID { get; set; }
        public int DoctorID { get; set; }
        public DateTime FechaHora { get; set; }
        public string Estado { get; set; } = "";
        public string? MotivoConsulta { get; set; }
        public string PacienteNombre { get; set; } = "";
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string DoctorNombre { get; set; } = "";
        public int CantAdjuntos { get; set; }
    }
}
