namespace Proyecto_JN_G7.Models
{
    public class Autenticacion
    {
        public long UsuarioID { get; set; }
        public string? Cedula { get; set; }
        public string? NombreCompleto { get; set; }
        public string? CorreoElectronico { get; set; }
        public string? ContrasenaHash { get; set; }
        public string? ConfirmarContrasenna { get; set; }
        public int RolID { get; set; }
        public string? NombreRol { get; set; }
    }
}
