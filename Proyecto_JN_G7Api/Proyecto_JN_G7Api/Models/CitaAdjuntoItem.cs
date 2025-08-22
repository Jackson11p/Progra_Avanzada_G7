namespace Proyecto_JN_G7Api.Models
{
    public class CitaAdjuntoItem
    {
        public int AdjuntoID { get; set; }
        public int CitaID { get; set; }
        public string NombreArchivo { get; set; } = "";
        public string RutaRelativa { get; set; } = ""; // p.ej. "uploads/citas/123/abc.pdf"
        public string ContentType { get; set; } = "";
        public long SizeBytes { get; set; }
        public DateTime FechaSubidaUtc { get; set; }
    }
}
