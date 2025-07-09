using Proyecto_JN_G7Api.Models;

namespace Proyecto_JN_G7Api.Services
{
    public interface IUtilitarios
    {
        RespuestaEstandar RespuestaCorrecta(object? contenido);

        RespuestaEstandar RespuestaIncorrecta(string mensaje);

        string GenerarContrasenna(int longitud);

        void EnviarCorreo(string destinatario, string asunto, string cuerpo);

        string Encrypt(string texto);
    }
}
