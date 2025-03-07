namespace MCTG.Models
{
    public class Card
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
        public int PackageNumber { get; set; }
    }
}