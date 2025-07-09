namespace YooVisitUserAPI.Dtos
{
    public class UserDto
    {
        public Guid IdUtilisateur { get; set; } // On utilise un Guid, c'est plus robuste qu'un int.
        public string Email { get; set; }
        public DateTime DateInscription { get; set; }
    }
}
