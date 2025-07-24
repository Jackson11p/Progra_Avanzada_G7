namespace Proyecto_JN_G7.Models
{
    public class HistorialMedico
    {
        public int HistorialID { get; set; }
        public long PacienteID { get; set; }
        public long DoctorID { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public Autenticacion? Paciente { get; set; }
        public Autenticacion? Doctor { get; set; }
    }
}