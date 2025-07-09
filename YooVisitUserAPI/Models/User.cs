namespace YooVisitUserAPI.Models;

public class User
{
    public Guid IdUtilisateur { get; set; }
    public string Email { get; set; }
    public string HashedPassword { get; set; } // Le mot de passe HACHÉ
    public DateTime DateInscription { get; set; }
}

