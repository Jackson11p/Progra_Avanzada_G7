namespace Proyecto_JN_G7.Models
{
    public class DoctorListItem
    {
        public int DoctorID { get; set; }
        public int? UsuarioID { get; set; }
        public string NombreCompleto { get; set; } = "";
        public string Especialidad { get; set; } = "";
        public string? CedulaProfesional { get; set; } = "";
    }
}
