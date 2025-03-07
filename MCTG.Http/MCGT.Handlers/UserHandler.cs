using MCTG.Models;
using MCTG.Repositories;
using Microsoft.Extensions.Logging;

namespace MCTG.Http.Handlers;

public class UserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ICardRepository _cardRepository;
    private readonly ILogger<UserHandler> _logger;

    public UserHandler(IUserRepository userRepository, ICardRepository cardRepository, ILogger<UserHandler> logger)
    {
        _userRepository = userRepository;
        _cardRepository = cardRepository;
        _logger = logger;
    }

    // UserHandler.cs
    public async Task<HttpResponse> HandleRegister(Dictionary<string, string> userDetails)
    {
        if (!userDetails.ContainsKey("Username") || !userDetails.ContainsKey("Password"))
        {
            return HttpResponse.BadRequest();
        }

        var user = new User
        {
            UserName = userDetails["Username"],
            Password = userDetails["Password"],
            Coins = 20, // Initial coins for new users
            ELO = 100 // Initial ELO for new users
        };

        try
        {
            await _userRepository.Create(user);
            return HttpResponse.Created();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("User with the same username already exists"))
        {
            _logger.LogWarning(ex, "User with the same username already exists");
            return HttpResponse.Conflict();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while registering user");
            return HttpResponse.InternalServerError();
        }
    }

    // UserHandler.cs
    public async Task<HttpResponse> HandleProfile(Dictionary<string, string> userDetails)
    {
        if (!userDetails.ContainsKey("Username"))
        {
            return HttpResponse.BadRequest();
        }

        var username = userDetails["Username"];
        try
        {
            var user = await _userRepository.GetByUserName(username);
            if (user == null)
            {
                return HttpResponse.NotFound("User not found");
            }

            return HttpResponse.UserProfileResponse(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while retrieving user profile");
            return HttpResponse.InternalServerError();
        }
    }
    public async Task<HttpResponse> HandleEditProfile(Dictionary<string, string> userDetails)
    {
        if (!userDetails.ContainsKey("Username") || !userDetails.ContainsKey("Token"))
        {
            return HttpResponse.BadRequest();
        }

        var username = userDetails["Username"];
        var token = userDetails["Token"];

        try
        {
            var user = await _userRepository.GetByUserName(username);
            if (user == null)
            {
                return HttpResponse.NotFound("User not found");
            }

            if (user.Token != token)
            {
                return HttpResponse.Unauthorized("Invalid token");
            }

            // Update user details
            if (userDetails.ContainsKey("Bio"))
            {
                user.BIO = userDetails["Bio"];
            }
            if (userDetails.ContainsKey("Image"))
            {
                user.Image = userDetails["Image"];
            }

            await _userRepository.Update(user);
            return HttpResponse.Ok("Profile updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while editing user profile");
            return HttpResponse.InternalServerError();
        }
    }
    public async Task<HttpResponse> HandleAddCardsToDeck(string token, List<string> cardIds)
    {
        var user = await _userRepository.GetByToken(token);
        if (user == null)
        {
            return HttpResponse.Unauthorized("Invalid token");
        }

        foreach (var cardId in cardIds)
        {
            await _cardRepository.SaveCardToDeck(user.Token, cardId);
        }

        return HttpResponse.Ok("Cards added to deck successfully");
    }

    public async Task<HttpResponse> HandleGetDeck(string token)
    {
        var user = await _userRepository.GetByToken(token);
        if (user == null)
        {
            return HttpResponse.Unauthorized("Invalid token");
        }

        var deck = await _cardRepository.GetDeck(user.Token);
        return HttpResponse.Ok(System.Text.Json.JsonSerializer.Serialize(deck));
    }
    }