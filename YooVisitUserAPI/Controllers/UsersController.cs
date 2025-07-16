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
    // On injectera nos services (ex: pour parler à la BDD, générer le token)
    // C'est le principe d'Inversion de Dépendance (le 'D' de SOLID)
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly UserDbContext _context;

    public UsersController(UserDbContext context, IUserService userService, ITokenService tokenService)
    {
        _context = context;
        _userService = userService;
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
        // 1. Récupérer l'utilisateur par email
        var user = await _userService.GetUserByEmailAsync(loginDto.Email);
        if (user == null)
        {
            // On renvoie une erreur 401 Unauthorized. Message générique pour la sécurité.
            return Unauthorized("Email ou mot de passe invalide.");
        }

        // 2. Vérifier le mot de passe haché
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.HashedPassword);
        if (!isPasswordValid)
        {
            return Unauthorized("Email ou mot de passe invalide.");
        }

        // 3. Si tout est bon, on génère le token JWT
        (string token, DateTime expiration) = _tokenService.GenerateJwtToken(user);

        // 4. On renvoie le token et les infos de l'user
        return Ok(new LoginResponseDto
        {
            Token = token,
            Expiration = expiration,
            User = new UserDto { /* mapper les champs de l'user vers le DTO */ }
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
    [HttpGet("me")]
    [Authorize] // <-- MAGIE ! Seuls les utilisateurs avec un token valide peuvent accéder ici.
    public async Task<ActionResult<UserDto>> GetMyProfile()
    {
        // ASP.NET Core nous donne accès à l'identité de l'appelant.
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userService.GetUserByIdAsync(Guid.Parse(userId));
        return Ok(user);
    }

    [Authorize] // Seul le joueur connecté peut voir ses propres stats
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
            UserName = user.Nom ?? user.Email.Split('@').First(),
            Experience = user.Experience,
            ExplorationProgress = progress,
            AccessPoints = userPhotosCount // Pour l'instant, 1 photo = 1 point d'accès
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

        // On met à jour les propriétés
        user.Nom = updateDto.Nom;
        user.Biographie = updateDto.Biographie;

        // On sauvegarde les changements dans la base de données
        await _context.SaveChangesAsync();

        return NoContent(); // 204 NoContent est une réponse standard pour un update réussi
    }
}
