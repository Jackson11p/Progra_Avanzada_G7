namespace Proyecto_JN_G7.Models
{

    public class Factura
    {
        public int FacturaID { get; set; }
        public int CitaID { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal Total { get; set; }
        public string EstadoPago { get; set; } = "Pendiente";
    }
}