using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using YooVisitUserAPI.Interfaces; // ou YooVisitUserAPI.Services
using YooVisitUserAPI.Models;

namespace YooVisitUserAPI.Services;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;

    public TokenService(IConfiguration config)
    {
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        _issuer = config["Jwt:Issuer"]!;
        _audience = config["Jwt:Audience"]!;
    }

    public (string token, DateTime expiration) GenerateJwtToken(UserApplication user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.IdUtilisateur.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
        };

        // --- LA MODIFICATION EST ICI ---
        // On utilise un algorithme tout aussi sécurisé mais qui requiert une clé moins longue.
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

        var expirationDate = DateTime.UtcNow.AddDays(7);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expirationDate,
            SigningCredentials = creds,
            Issuer = _issuer,
            Audience = _audience
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expirationDate);
    }
}
