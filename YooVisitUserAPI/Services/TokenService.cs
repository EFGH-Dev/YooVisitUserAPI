using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using YooVisitUserAPI.Interfaces;
using YooVisitUserAPI.Models;

namespace YooVisitUserAPI.Services;

public class TokenService : ITokenService
{
    // On a besoin de la clé secrète et des infos de l'émetteur (Issuer).
    // On les récupère depuis appsettings.json via IConfiguration.
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;

    public TokenService(IConfiguration config)
    {
        // On va chercher les valeurs dans notre configuration
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        _issuer = config["Jwt:Issuer"]!;
        _audience = config["Jwt:Audience"]!;
    }

    public (string token, DateTime expiration) GenerateJwtToken(UserApplication user)
    {
        // 1. Les "Claims" sont les informations que l'on grave sur la clé (l'identité de l'utilisateur).
        // C'est la "fiche de personnage" embarquée dans le token.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.IdUtilisateur.ToString()), // L'ID de l'utilisateur (standard)
            new(JwtRegisteredClaimNames.Email, user.Email),                     // L'email (standard)
            // Tu pourrais ajouter des rôles ici : new Claim(ClaimTypes.Role, "Admin")
        };

        // 2. On crée les "crédentiels" de signature. C'est le sceau magique.
        // On utilise notre clé secrète et l'algorithme de hachage le plus puissant (Sha512).
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var expirationDate = DateTime.UtcNow.AddDays(7); // La clé expirera dans 7 jours.

        // 3. On assemble la clé (le token).
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),    // La fiche de perso
            Expires = expirationDate,                // La date d'expiration
            SigningCredentials = creds,              // Le sceau
            Issuer = _issuer,                        // Le nom de la forge
            Audience = _audience                     // Le royaume cible
        };

        // 4. On utilise un "Token Handler" pour écrire l'objet token en une chaîne de caractères.
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // 5. On renvoie la clé sous forme de texte et sa date d'expiration.
        return (tokenHandler.WriteToken(token), expirationDate);
    }
}