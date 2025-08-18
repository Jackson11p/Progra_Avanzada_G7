// Proyecto_JN_G7Api/Models/Usuarios.cs
namespace Proyecto_JN_G7Api.Models
{
    public class UsuarioCrear
    {
        public string Cedula { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string CorreoElectronico { get; set; } = "";
        public string Contrasena { get; set; } = "";
        public int RolID { get; set; } = 1; // 1 = Usuario por defecto
    }
    public class UsuarioActualizar
    {
        public string Cedula { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string CorreoElectronico { get; set; } = "";
        public int RolID { get; set; } = 1;
        public bool Activo { get; set; } = true;
    }

    public class CambiarRol
    {
        public int RolID { get; set; }
    }

    public class ResetPassword
    {
        public string Contrasena { get; set; } = "";
    }

    public class UsuarioListItem
    {
        public int UsuarioID { get; set; }
        public string Cedula { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string CorreoElectronico { get; set; } = "";
        public int RolID { get; set; }
        public string NombreRol { get; set; } = "";
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class UsuarioDetalle : UsuarioListItem { }

    public class RolItem
    {
        public int RolID { get; set; }
        public string NombreRol { get; set; } = "";
    }
}
