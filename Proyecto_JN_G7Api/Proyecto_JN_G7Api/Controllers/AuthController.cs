using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _cfg;
    public AuthController(IConfiguration cfg) => _cfg = cfg;

    public record LoginDto(string Email, string Password);
    public record TokenResponse(string token, DateTime expiresAt, string nombre, string rol, int usuarioId);

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        using var conn = new SqlConnection(_cfg.GetConnectionString("Connection"));
        var user = await conn.QueryFirstOrDefaultAsync<dynamic>(@"
            SELECT TOP 1 U.UsuarioID, U.NombreCompleto, U.CorreoElectronico,
                   U.ContrasenaHash, R.NombreRol
            FROM Usuarios U
            JOIN Roles R ON U.RolID = R.RolID
            WHERE U.CorreoElectronico = @Email AND U.Activo = 1;",
            new { dto.Email });

        if (user is null) return Unauthorized("Credenciales inválidas");

        using var sha = System.Security.Cryptography.SHA256.Create();
        var inputHash = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(dto.Password)))
                         .Replace("-", string.Empty);
        if (!string.Equals((string)user.ContrasenaHash, inputHash, StringComparison.OrdinalIgnoreCase))
            return Unauthorized("Credenciales inválidas");

        var jwt = _cfg.GetSection("JWT");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(2);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, ((int)user.UsuarioID).ToString()),
            new Claim(ClaimTypes.Name, (string)user.NombreCompleto),
            new Claim(ClaimTypes.Email, (string)user.CorreoElectronico),
            new Claim(ClaimTypes.Role, (string)user.NombreRol)
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var jwtString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new TokenResponse(jwtString, expires, (string)user.NombreCompleto, (string)user.NombreRol, (int)user.UsuarioID));
    }
}
