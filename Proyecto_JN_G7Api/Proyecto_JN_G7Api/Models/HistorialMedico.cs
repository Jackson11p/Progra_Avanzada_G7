using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Proyecto_JN_G7Api.Models
{
    public class HistorialMedico
    {
        public long HistorialMedicoID { get; set; }
        public long PacienteID { get; set; }
        public long DoctorID { get; set; }
        public long DiagnosticoID { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
