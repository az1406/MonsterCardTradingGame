namespace MCTG.Models
{
    public class User
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string BIO { get; set; } = string.Empty;
        public int ELO { get; set; }
        public int Coins { get; set; }
        public string Token { get; set; } = string.Empty;
        public int GamesPlayed { get; set; }
        public string Image { get; set; } = string.Empty;
    }
}