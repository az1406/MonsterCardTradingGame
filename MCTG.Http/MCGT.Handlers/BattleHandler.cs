// File: MCTG.Http/MCGT.Handlers/BattleHandler.cs

using MCTG.Models;
using MCTG.Repositories;
using Microsoft.Extensions.Logging;

namespace MCTG.Http.Handlers
{
    public class BattleHandler
    {
        private readonly IBattleRepository _battleRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICardRepository _cardRepository;
        private readonly ILogger<BattleHandler> _logger;

        public BattleHandler(IBattleRepository battleRepository, IUserRepository userRepository,
            ICardRepository cardRepository, ILogger<BattleHandler> logger)
        {
            _battleRepository = battleRepository;
            _userRepository = userRepository;
            _cardRepository = cardRepository;
            _logger = logger;
        }

        public async Task<HttpResponse> HandleBattle(string userToken, string opponentToken)
        {
            var user = await _userRepository.GetByToken(userToken);
            if (user == null)
            {
                return HttpResponse.Unauthorized("Invalid user token");
            }

            var opponent = await _userRepository.GetByToken(opponentToken);
            if (opponent == null)
            {
                return HttpResponse.BadRequest("Opponent not found");
            }

            var userDeck = await _cardRepository.GetDeck(userToken);
            var opponentDeck = await _cardRepository.GetDeck(opponentToken);

            if (userDeck.Count == 0 || opponentDeck.Count == 0)
            {
                return HttpResponse.BadRequest("One or both users have empty decks");
            }

            var battle = new Battle
            {
                User1Token = user.Token,
                User2Token = opponent.Token,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddMinutes(1),
                WinnerToken = string.Empty,
                IsDraw = false,
                Rounds = 0,
                User1Wins = 0,
                User2Wins = 0
            };

            battle = await _battleRepository.CreateBattle(battle);

            HttpResponse.Ok($"Battle started with ID: {battle.Id}");

            while (battle.Rounds < 100 && userDeck.Count > 0 && opponentDeck.Count > 0)
            {
                var userCard = SelectRandomCard(userDeck);
                var opponentCard = SelectRandomCard(opponentDeck);

                var winnerToken = DetermineWinner(userCard, opponentCard, userToken, opponentToken);

                if (winnerToken == userToken)
                {
                    battle.User1Wins++;
                }
                else if (winnerToken == opponentToken)
                {
                    battle.User2Wins++;
                }

                battle.Rounds++;
            }

            if (battle.User1Wins > battle.User2Wins)
            {
                battle.WinnerToken = userToken;
                battle.IsDraw = false;
            }
            else if (battle.User2Wins > battle.User1Wins)
            {
                battle.WinnerToken = opponentToken;
                battle.IsDraw = false;
            }
            else
            {
                battle.WinnerToken = string.Empty;
                battle.IsDraw = true;
            }

            battle.EndTime = DateTime.UtcNow;

            await _battleRepository.UpdateBattleResult(battle);

            _logger.LogInformation($"Battle updated with ID: {battle.Id} between {user.UserName} and {opponent.UserName}");

            var battleStatistics = GetBattleStatistics(battle, user.UserName, opponent.UserName);

            return HttpResponse.Ok(battleStatistics);
        }

        private string GetBattleStatistics(Battle battle, string userName, string opponentName)
        {
            return $"Battle ID: {battle.Id}\n" +
                   $"User 1: {userName}\n" +
                   $"User 2: {opponentName}\n" +
                   $"Total Rounds: {battle.Rounds}\n" +
                   $"Rounds won by {userName}: {battle.User1Wins}\n" +
                   $"Rounds won by {opponentName}: {battle.User2Wins}\n" +
                   $"Winner: {(battle.IsDraw ? "Draw" : battle.WinnerToken == battle.User1Token ? userName : opponentName)}";
        }

        private Card SelectRandomCard(List<Card> deck)
        {
            var random = new Random();
            int index = random.Next(deck.Count);
            return deck[index];
        }

        private string DetermineWinner(Card userCard, Card opponentCard, string userToken, string opponentToken)
        {
            if (userCard.Damage > opponentCard.Damage)
            {
                return userToken;
            }
            else if (userCard.Damage < opponentCard.Damage)
            {
                return opponentToken;
            }
            else
            {
                return "Draw";
            }
        }
    }
}