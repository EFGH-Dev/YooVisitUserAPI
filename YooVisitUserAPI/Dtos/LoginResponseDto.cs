namespace YooVisitUserAPI.Dtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; } // Le fameux Token JWT ! La clé du donjon.
        public DateTime Expiration { get; set; }
        public UserDto User { get; set; } // On renvoie aussi les infos de l'utilisateur.
    }
}
