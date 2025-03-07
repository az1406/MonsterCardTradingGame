using MCTG.Models;

namespace MCTG.Repositories;

public interface ICardRepository
{
    Task<Card?> GetById(string id);
    Task Create(Card card);
    Task<int> GetNextPackageNumber();
    Task<List<Card>> GetCardsByPackageNumber(int packageNumber);
    Task SaveCardToStack(string userToken, string cardId, int packageNumber);
    Task<int> GetCurrentPackageNumber();
    Task<List<Card>> GetDeck(string userToken);
    Task SaveCardToDeck(string userToken, string cardId); // Add this method
}