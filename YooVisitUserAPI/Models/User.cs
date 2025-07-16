using System.ComponentModel.DataAnnotations; // <-- AJOUTER CE USING !

namespace YooVisitUserAPI.Models;

public class UserApplication
{
    [Key]
    public Guid IdUtilisateur { get; set; }

    public string Email { get; set; }

    public string HashedPassword { get; set; }

    public DateTime DateInscription { get; set; }
    public int Experience { get; set; } = 0;
    [StringLength(50)]
    public string? Nom { get; set; }

    [StringLength(500)]
    public string? Biographie { get; set; }
}
