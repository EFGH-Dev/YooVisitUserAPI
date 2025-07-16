using System.ComponentModel.DataAnnotations;

namespace YooVisitUserAPI.Dtos
{
    public class UserDto
    {
        public Guid IdUtilisateur { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Nom { get; set; }
        public string? Biographie { get; set; }
        public int Experience { get; set; }
        public DateTime DateInscription { get; set; }
    }
}
