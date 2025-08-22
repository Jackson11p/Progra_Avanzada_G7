namespace Proyecto_JN_G7Api.Models
{
    public class CitaAdjuntoCreateResult
    {
        public int AdjuntoID { get; set; }
        public string NombreArchivo { get; set; } = "";
        public string RutaRelativa { get; set; } = "";
        public string Url { get; set; } = ""; 
        public string ContentType { get; set; } = "";
        public long SizeBytes { get; set; }
    }
}

