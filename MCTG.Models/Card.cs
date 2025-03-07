namespace MCTG.Models
{
    public class Card
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ElementType { get; set; } = string.Empty;
        public int PackageNumber { get; set; }
        public bool IsSpell { get; set; }
        
        public double Damage { get; set; }
    }
}