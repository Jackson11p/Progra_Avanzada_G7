namespace Proyecto_JN_G7.Models
{

    public class Factura
    {
        public int FacturaID { get; set; }
        public long PacienteID { get; set; }   
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public Autenticacion? Paciente { get; set; }
    }
}