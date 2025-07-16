using System.ComponentModel.DataAnnotations;

namespace YooVisitUserAPI.Dtos
{
    public class UpdateProfileDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Nom { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Biographie { get; set; }
    }
}
