using System.ComponentModel.DataAnnotations;

namespace Proyecto_JN_G7Api.Models
{
    public class Contacto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "El asunto es obligatorio")]
        public string? Asunto { get; set; }

        [Required(ErrorMessage = "El mensaje es obligatorio")]
        public string? Mensaje { get; set; }
    }
}
