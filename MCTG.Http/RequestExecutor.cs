using System.Text.Json;
using MCTG.Http.Handlers;
using MCTG.Models;
using MCTG.Repositories;
using Microsoft.Extensions.Logging;

namespace MCTG.Http;

public class RequestExecutor
{
    private readonly UserHandler _userHandler;
    private readonly SessionHandler _sessionHandler;
    private readonly PackageHandler _packageHandler;
    private readonly TransactionHandler _transactionHandler;
    private readonly IUserRepository _userRepository;
    private readonly ICardRepository _cardRepository;
    private readonly ILogger<RequestExecutor> _logger;

    public RequestExecutor(UserHandler userHandler, SessionHandler sessionHandler, PackageHandler packageHandler, TransactionHandler transactionHandler, IUserRepository userRepository, ICardRepository cardRepository, ILogger<RequestExecutor> logger)
    {
        _userHandler = userHandler;
        _sessionHandler = sessionHandler;
        _packageHandler = packageHandler;
        _transactionHandler = transactionHandler;
        _userRepository = userRepository;
        _cardRepository = cardRepository;
        _logger = logger;
    }

    public async Task<HttpResponse> ProcessAsync(string[] requestLines)
    {
        if (requestLines.Length > 0)
        {
            string[] requestLine = requestLines[0].Split(" ");
            if (requestLine.Length >= 2)
            {
                string method = requestLine[0];
                string path = requestLine[1];

                _logger.LogInformation($"Received request: {method} {path}");

                switch (method)
                {
                    case "POST":
                        var postRequestBody = requestLines.Last();
                        if (path == "/packages")
                        {
                            return await HandlePostPackages(postRequestBody, requestLines);
                        }
                        else if (path == "/transactions/packages")
                        {
                            return await HandlePostTransactionsPackages(requestLines);
                        }
                        else
                        {
                            Dictionary<string, string>? postUserDetails;
                            try
                            {
                                postUserDetails = JsonSerializer.Deserialize<Dictionary<string, string>>(postRequestBody);
                            }
                            catch (JsonException ex)
                            {
                                _logger.LogError(ex, "Error deserializing JSON request body");
                                return HttpResponse.BadRequest("Invalid JSON format");
                            }
                            if (postUserDetails == null)
                            {
                                return HttpResponse.BadRequest("Request body is empty or invalid");
                            }
                            return await HandlePostRequest(path, postUserDetails);
                        }
                    case "GET":
                        return await HandleGetRequest(path, requestLines);
                    case "PUT":
                        var putRequestBody = requestLines.Last();
                        if (path == "/deck")
                        {
                            List<string>? cardIds;
                            try
                            {
                                cardIds = JsonSerializer.Deserialize<List<string>>(putRequestBody);
                            }
                            catch (JsonException ex)
                            {
                                _logger.LogError(ex, "Error deserializing JSON request body");
                                return HttpResponse.BadRequest("Invalid JSON format");
                            }
                            if (cardIds == null || cardIds.Count == 0)
                            {
                                return HttpResponse.BadRequest("Request body is empty or invalid");
                            }
                            string? token = GetTokenFromHeaders(requestLines);
                            if (token == null)
                            {
                                return HttpResponse.Unauthorized("Authorization header is missing or invalid");
                            }
                            return await _userHandler.HandleAddCardsToDeck(token, cardIds);
                        }
                        else
                        {
                            Dictionary<string, string>? putUserDetails;
                            try
                            {
                                putUserDetails = JsonSerializer.Deserialize<Dictionary<string, string>>(putRequestBody);
                            }
                            catch (JsonException ex)
                            {
                                _logger.LogError(ex, "Error deserializing JSON request body");
                                return HttpResponse.BadRequest("Invalid JSON format");
                            }
                            if (putUserDetails == null)
                            {
                                return HttpResponse.BadRequest("Request body is empty or invalid");
                            }
                            return await HandlePutRequest(path, putUserDetails, requestLines);
                        }
                    default:
                        return HttpResponse.NotFound();
                }
            }
        }

        return HttpResponse.BadRequest();
    }

    private async Task<HttpResponse> HandlePostTransactionsPackages(string[] requestLines)
    {
        string? token = GetTokenFromHeaders(requestLines);
        if (token == null)
        {
            return HttpResponse.Unauthorized("Authorization header is missing or invalid");
        }

        return await _transactionHandler.HandlePurchasePackage(token);
    }

    private async Task<HttpResponse> HandlePostPackages(string requestBody, string[] requestLines)
    {
        string? token = GetTokenFromHeaders(requestLines);
        if (token == null)
        {
            return HttpResponse.Unauthorized("Authorization header is missing or invalid");
        }

        var user = await _userRepository.GetByToken(token);
        if (user == null || user.UserName != "admin")
        {
            return HttpResponse.Unauthorized("Only admin can create packages");
        }

        List<Card>? cards;
        try
        {
            cards = JsonSerializer.Deserialize<List<Card>>(requestBody);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing JSON request body");
            return HttpResponse.BadRequest("Invalid JSON format");
        }
        if (cards == null || cards.Count == 0)
        {
            return HttpResponse.BadRequest("Request body is empty or invalid");
        }

        try
        {
            int packageNumber = await _cardRepository.GetNextPackageNumber();
            foreach (var card in cards)
            {
                card.PackageNumber = packageNumber;
                await _cardRepository.Create(card);
            }
            return HttpResponse.Created("Package created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating package");
            return HttpResponse.InternalServerError();
        }
    }

    private async Task<HttpResponse> HandlePostRequest(string path, Dictionary<string, string> userDetails)
    {
        switch (path)
        {
            case "/users":
                return await _userHandler.HandleRegister(userDetails);
            case "/sessions":
                return await _sessionHandler.HandleLogin(userDetails);

            default:
                return HttpResponse.NotFound();
        }
    }

    private async Task<HttpResponse> HandleGetRequest(string path, string[] requestLines)
    {
        if (path.StartsWith("/users/"))
        {
            var username = path.Substring("/users/".Length);
            var user = await _userRepository.GetByUserName(username);

            if (user == null)
            {
                return HttpResponse.NotFound("User not found");
            }

            string? token = GetTokenFromHeaders(requestLines);
            if (token == null || !CheckAuthorization(token, user.Token))
            {
                return HttpResponse.Unauthorized("Authorization header is missing or invalid");
            }

            return await _userHandler.HandleProfile(new Dictionary<string, string> { { "Username", username }, { "Token", token } });
        }
        else if (path == "/deck")
        {
            string? token = GetTokenFromHeaders(requestLines);
            if (token == null)
            {
                return HttpResponse.Unauthorized("Authorization header is missing or invalid");
            }

            return await _userHandler.HandleGetDeck(token);
        }
        return HttpResponse.NotFound();
    }

    private async Task<HttpResponse> HandlePutRequest(string path, Dictionary<string, string> userDetails, string[] requestLines)
    {
        if (path.StartsWith("/users/"))
        {
            var username = path.Substring("/users/".Length);
            var user = await _userRepository.GetByUserName(username);

            if (user == null)
            {
                return HttpResponse.NotFound("User not found");
            }

            string? token = GetTokenFromHeaders(requestLines);
            if (token == null || !CheckAuthorization(token, user.Token))
            {
                return HttpResponse.Unauthorized("Authorization header is missing or invalid");
            }

            userDetails["Username"] = username;
            userDetails["Token"] = token;
            var response = await _userHandler.HandleEditProfile(userDetails);

            if (response.StatusCode == 200)
            {
                return HttpResponse.Ok();
            }

            return response;
        }
        return HttpResponse.NotFound();
    }

    private bool CheckAuthorization(string token, string userToken)
    {
        return token == userToken;
    }

    private string? GetTokenFromHeaders(string[] requestLines)
    {
        foreach (var line in requestLines)
        {
            if (line.StartsWith("Authorization: Bearer "))
            {
                return line.Substring("Authorization: Bearer ".Length);
            }
        }
        return null;
    }
}