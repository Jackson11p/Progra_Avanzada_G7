namespace Proyecto_JN_G7.Models
{

    public class Factura
    {
        public int FacturaID { get; set; }
        public int CitaID { get; set; }
        public string Paciente { get; set; } = string.Empty;
        public string Doctor { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string MotivoConsulta { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string EstadoPago { get; set; } = "Pendiente";
    }
}