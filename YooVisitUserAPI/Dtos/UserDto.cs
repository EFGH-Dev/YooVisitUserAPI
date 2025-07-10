namespace YooVisitUserAPI.Dtos
{
    public class UserDto
    {
        public Guid IdUtilisateur { get; set; }

        public string Email { get; set; }

        public string HashedPassword { get; set; }

        public DateTime DateInscription { get; set; }
        public int Experience { get; set; } = 0;
    }
}
