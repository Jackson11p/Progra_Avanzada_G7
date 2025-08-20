namespace Proyecto_JN_G7Api.Models
{
    public class PacienteAdminItem
    {
        public int PacienteID { get; set; }
        public int UsuarioID { get; set; }
        public string Cedula { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string CorreoElectronico { get; set; } = "";
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }
    }
}
