using MCTG.Models;
using MCTG.Repositories;
using Microsoft.Extensions.Logging;

namespace MCTG.Http.Handlers;

public class PackageHandler
{
    private readonly ICardRepository _cardRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PackageHandler> _logger;

    public PackageHandler(ICardRepository cardRepository, IUserRepository userRepository, ILogger<PackageHandler> logger)
    {
        _cardRepository = cardRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<HttpResponse> HandleCreatePackage(Dictionary<string, string> packageDetails, string adminToken)
    {
        if (!packageDetails.ContainsKey("Cards") || packageDetails["Cards"].Split(',').Length != 5)
        {
            return HttpResponse.BadRequest("Package must contain exactly 5 cards.");
        }

        var adminUser = await _userRepository.GetByToken(adminToken);
        if (adminUser == null || adminUser.UserName != "admin")
        {
            return HttpResponse.Unauthorized("Only admin can create packages.");
        }

        var cards = packageDetails["Cards"].Split(',');
        var packageNumber = await _cardRepository.GetNextPackageNumber();

        foreach (var cardDetails in cards)
        {
            var cardInfo = cardDetails.Split('|');
            if (cardInfo.Length != 5)
            {
                return HttpResponse.BadRequest("Each card must have an ID, name, element type, spell status, and damage.");
            }

            var card = new Card
            {
                Id = cardInfo[0],
                Name = cardInfo[1],
                ElementType = cardInfo[2],
                IsSpell = bool.Parse(cardInfo[3]),
                Damage = double.Parse(cardInfo[4]),
                PackageNumber = packageNumber
            };

            var existingCard = await _cardRepository.GetById(card.Id);
            if (existingCard != null)
            {
                return HttpResponse.Conflict($"Card with ID {card.Id} already exists.");
            }

            await _cardRepository.Create(card);
        }

        return HttpResponse.Created();
    }
}