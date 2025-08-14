using System.Net.Http.Headers;

namespace Proyecto_JN_G7.Services
{
    public class TokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _http;

        public TokenHandler(IHttpContextAccessor http) => _http = http;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var token = _http.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return base.SendAsync(request, ct);
        }
    }
}
