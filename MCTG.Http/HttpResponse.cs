using MCTG.Models;

namespace MCTG.Http;

public class HttpResponse
{
    public required int StatusCode { get; set; }
    public required string ContentType { get; set; }
    public required string StatusMessage { get; set; }
    public required string Content { get; set; }

    public static HttpResponse Ok(string? content = null)
    {
        return new HttpResponse
        {
            StatusCode = 200,
            ContentType = "application/json",
            StatusMessage = "OK",
            Content = content ?? string.Empty
        };
    }

    public static HttpResponse Created(string? content = null)
    {
        return new HttpResponse
        {
            StatusCode = 201,
            ContentType = "application/json",
            StatusMessage = "Created",
            Content = content ?? "Successfully created"
        };
    }

    public static HttpResponse BadRequest(string? content = null)
    {
        return new HttpResponse
        {
            StatusCode = 400,
            ContentType = "application/json",
            StatusMessage = "Bad Request",
            Content = content ?? "The request was invalid"
        };
    }

    public static HttpResponse Unauthorized(string? content = null)
    {
        return new HttpResponse
        {
            StatusCode = 401,
            ContentType = "application/json",
            StatusMessage = "Unauthorized",
            Content = content ?? "Authentication is required and has failed or has not yet been provided"
        };
    }

    public static HttpResponse NotFound(string? content = null)
    {
        return new HttpResponse
        {
            StatusCode = 404,
            ContentType = "application/json",
            StatusMessage = "Not Found",
            Content = content ?? "The requested resource was not found"
        };
    }

    public static HttpResponse InternalServerError(string? content = null)
    {
        return new HttpResponse
        {
            StatusCode = 500,
            ContentType = "application/json",
            StatusMessage = "Internal Server Error",
            Content = content ?? "An internal server error occurred"
        };
    }

    public static HttpResponse Conflict(string? content = null)
    {
        return new HttpResponse
        {
            StatusCode = 409,
            ContentType = "application/json",
            StatusMessage = "Conflict",
            Content = content ?? "User already exists"
        };
    }

    public static HttpResponse TokenResponse(string token, string username)
    {
        return new HttpResponse
        {
            StatusCode = 200,
            ContentType = "application/json",
            StatusMessage = "OK",
            Content = $"{username}-mtcgToken: {token}"
        };
    }
    public static HttpResponse UserProfileResponse(User user)
    {
        var userProfile = new
        {
            user.UserName,
            user.BIO,
            user.ELO,
            user.Coins,
            user.GamesPlayed,
            user.Image
        };

        var content = System.Text.Json.JsonSerializer.Serialize(userProfile);
        return new HttpResponse
        {
            StatusCode = 200,
            ContentType = "application/json",
            StatusMessage = "OK",
            Content = content
        };
    }

    public string ToResponseString()
    {
        return $"HTTP/1.1 {StatusCode} {Content}\r\n";
    }
}