namespace MCTG.Models
{
    public class Battle
    {
        public int Id { get; set; }
        public string User1Token { get; set; } = string.Empty;
        public string User2Token { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string WinnerToken { get; set; } = string.Empty;
        public bool IsDraw { get; set; }
        public int Rounds { get; set; }
        public int User1Wins { get; set; }
        public int User2Wins { get; set; }
    }
}