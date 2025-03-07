// Handlers/SessionHandler.cs
using MCTG.Models;
using MCTG.Repositories;
using Microsoft.Extensions.Logging;

namespace MCTG.Http.Handlers;

public class SessionHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SessionHandler> _logger;

    public SessionHandler(IUserRepository userRepository, ILogger<SessionHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<HttpResponse> HandleLogin(Dictionary<string, string> userDetails)
    {
        if (!userDetails.ContainsKey("Username") || !userDetails.ContainsKey("Password"))
        {
            return HttpResponse.BadRequest();
        }

        var userName = userDetails["Username"];
        var password = userDetails["Password"];

        try
        {
            var user = await _userRepository.GetByUserName(userName);
            if (user == null || user.Password != password)
            {
                return HttpResponse.BadRequest();
            }
            
            // Generate token
            user.Token = Guid.NewGuid().ToString();
            await _userRepository.Update(user);

            return HttpResponse.TokenResponse(user.Token, user.UserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while logging in user");
            return HttpResponse.InternalServerError();
        }
    }
}