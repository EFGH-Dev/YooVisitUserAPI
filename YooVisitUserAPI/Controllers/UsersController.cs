using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using YooVisitUserAPI.Data;
using YooVisitUserAPI.DTO;
using YooVisitUserAPI.Dtos;
using YooVisitUserAPI.Interfaces;
using YooVisitUserAPI.Services;

namespace YooVisitUserAPI.Controllers;

[ApiController]
[Route("api/[controller]")] // L'URL de base sera /api/users
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly UserDbContext _context;

    public UsersController(UserDbContext context, IUserService userService, ITokenService tokenService)
    {
        _context = context;
        _userService = userService; // On assigne le service
        _tokenService = tokenService;
    }

    // --- ENDPOINTS ---

    // POST /api/users/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
    {
        // 1. Vérifier si l'utilisateur existe déjà
        var existingUser = await _userService.GetUserByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            // On renvoie une erreur 400 Bad Request
            return BadRequest("Cet e-mail est déjà utilisé. Le pseudo est déjà pris !");
        }

        // 2. Hacher le mot de passe. ON NE STOCKE JAMAIS UN MOT DE PASSE EN CLAIR !
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        // 3. Créer l'utilisateur (logique dans le _userService)
        var newUser = await _userService.CreateUserAsync(registerDto, hashedPassword);

        // 4. On retourne une réponse HTTP 201 Created avec les infos de l'utilisateur créé.
        return CreatedAtAction(nameof(GetUserById), new { id = newUser.IdUtilisateur }, newUser);
    }

    // POST /api/users/login
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.HashedPassword))
        {
            return Unauthorized("Email ou mot de passe invalide.");
        }

        (string token, DateTime expiration) = _tokenService.GenerateJwtToken(user);
        
        // On "map" manuellement l'entité UserApplication vers le UserDto sécurisé
        var userDto = new UserDto
        {
            IdUtilisateur = user.IdUtilisateur,
            Email = user.Email,
            Nom = user.Nom,
            Biographie = user.Biographie,
            Experience = user.Experience,
            DateInscription = user.DateInscription
        };

        return Ok(new LoginResponseDto
        {
            Token = token,
            Expiration = expiration,
            User = userDto // On renvoie le DTO sécurisé
        });
    }

    // GET /api/users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        // Logique pour récupérer un utilisateur par son ID...
        // ...
        return Ok(/* l'utilisateur trouvé */);
    }

    // GET /api/users/me (Exemple d'endpoint protégé)
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMyProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        // On crée manuellement le DTO pour être sûr d'inclure les bons champs
        var userDto = new UserDto
        {
            IdUtilisateur = user.IdUtilisateur,
            Email = user.Email,
            DateInscription = user.DateInscription,
            Nom = user.Nom, // On inclut le nom personnalisé
            Biographie = user.Biographie // On inclut la biographie
        };

        return Ok(userDto);
    }

    [Authorize]
    [HttpGet("my-stats")]
    public async Task<ActionResult<PlayerStatsDto>> GetMyStats()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound("Utilisateur non trouvé.");
        }

        var totalPhotosInWorld = await _context.Photos.CountAsync();
        var userPhotosCount = await _context.Photos.CountAsync(p => p.UserId == userId);

        double progress = (totalPhotosInWorld > 0) ? (double)userPhotosCount / totalPhotosInWorld : 0;

        var stats = new PlayerStatsDto
        {
            // On utilise le Nom en priorité, sinon la partie de l'email
            UserName = user.Nom ?? user.Email.Split('@').First(),
            Experience = user.Experience,
            ExplorationProgress = progress,
            AccessPoints = userPhotosCount
        };

        return Ok(stats);
    }

    [Authorize]
    [HttpPut("me")] // On utilise PUT pour la mise à jour d'une ressource existante
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto updateDto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound("Utilisateur non trouvé.");
        }

        // On met à jour les propriétés SEULEMENT si elles sont fournies dans la requête.
        // Si la chaîne n'est pas nulle, on met à jour. Cela permet de passer une chaîne vide pour effacer le nom.
        if (updateDto.Nom != null)
        {
            user.Nom = updateDto.Nom;
        }

        // On fait de même pour la biographie.
        if (updateDto.Biographie != null)
        {
            user.Biographie = updateDto.Biographie;
        }

        // On sauvegarde les changements dans la base de données
        await _context.SaveChangesAsync();

        return NoContent(); // 204 NoContent est une réponse standard pour un update réussi
    }
}
