using MCTG.Models;
using MCTG.Repositories;
using Microsoft.Extensions.Logging;

namespace MCTG.Http.Handlers;

public class TransactionHandler
{
    private readonly ICardRepository _cardRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<TransactionHandler> _logger;

    public TransactionHandler(ICardRepository cardRepository, IUserRepository userRepository, ILogger<TransactionHandler> logger)
    {
        _cardRepository = cardRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<HttpResponse> HandlePurchasePackage(string token)
    {
        var user = await _userRepository.GetByToken(token);
        if (user == null)
        {
            return HttpResponse.Unauthorized("Invalid token");
        }

        if (user.Coins < 5)
        {
            return HttpResponse.BadRequest("Not enough coins to buy a package");
        }

        var packageNumber = await _cardRepository.GetCurrentPackageNumber();

        var cards = await _cardRepository.GetCardsByPackageNumber(packageNumber);
        if (cards == null || cards.Count == 0)
        {
            return HttpResponse.BadRequest("No cards available in the package");
        }

        user.Coins -= 5;
        await _userRepository.Update(user);

        foreach (var card in cards)
        {
            await _cardRepository.SaveCardToStack(user.Token, card.Id, packageNumber);
        }

        return HttpResponse.Ok("Package purchased successfully");
    }
}