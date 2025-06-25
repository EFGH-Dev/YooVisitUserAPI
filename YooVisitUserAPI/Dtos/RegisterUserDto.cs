using System.ComponentModel.DataAnnotations;

namespace YooVisitUserAPI.DTO
{
    public class RegisterUserDto
    {
        // On utilise les annotations pour la validation automatique !
        [Required]
        [StringLength(50)]
        public string Nom { get; set; }

        [Required]
        [EmailAddress] // ASP.NET Core vérifie que c'est un format d'email valide.
        public string Email { get; set; }

        [Required]
        [MinLength(8)] // On impose un mot de passe d'au moins 8 caractères.
        public string Password { get; set; }
    }
}
