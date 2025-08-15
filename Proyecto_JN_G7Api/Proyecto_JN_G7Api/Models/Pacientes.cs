namespace Proyecto_JN_G7Api.Models
{
    public class Pacientes
    {
        public string Cedula { get; set; } = "";                
        public string? NombreCompleto { get; set; }
        public DateTime? FechaNacimiento { get; set; }          
        public string? Genero { get; set; }                      
        public string? Direccion { get; set; }                   
        public string? Telefono { get; set; }                   
        public string? CorreoElectronico { get; set; }
        public string? Contrasena { get; set; }
    }
}
