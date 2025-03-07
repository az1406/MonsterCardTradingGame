using System.Data.Common;
using Microsoft.Extensions.Logging;
using MCTG.Database;
using MCTG.Models;

namespace MCTG.Repositories.Postgres;

public class NpgsqlUserRepository(DatabaseConfig databaseConfig, ILogger<NpgsqlUserRepository> logger)
    : AbstractRepository(databaseConfig), IUserRepository
{
    public async ValueTask<User?> GetByUserName(string name)
    {
        try
        {
            await using DbConnection connection = CreateConnection();
            await connection.OpenAsync();
            User? user =
                await connection.QuerySingleAsync<User>("SELECT * FROM users WHERE username = @name", new { name });

            return user;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while getting user by name");
            throw;
        }
    }

    public async ValueTask<User> Create(User user)
{
    try
    {
        await using DbConnection connection = CreateConnection();
        await connection.OpenAsync();

        // Check if user already exists
        User? existingUser = await GetByUserName(user.UserName);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with the same username already exists");
        }

        await using DbCommand command = connection.CreateCommand();
        command.CommandText = "INSERT INTO users (username, password, token, bio, image, games_played, elo, coins) VALUES (@username, @password, @token, @bio, @image, @games_played, @elo, @coins)";
        command.AddParameters(new { username = user.UserName, password = user.Password, token = user.Token, bio = user.BIO, image = user.Image, games_played = user.GamesPlayed, elo = user.ELO, coins = user.Coins });
        await command.ExecuteNonQueryAsync();

        return connection.QuerySingleAsync<User>("SELECT * FROM users WHERE username = @username", new { username = user.UserName }).Result!;
    }
    catch (Exception e)
    {
        logger.LogError(e, "Error while creating user");
        throw;
    }
}

public async ValueTask<User> Update(User user)
{
    try
    {
        await using DbConnection connection = CreateConnection();
        await connection.OpenAsync();
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = "UPDATE users SET password = @password, token = @token, coins = @coins, elo = @elo, games_played = @games_played, bio = @bio, image = @image WHERE username = @username";
        command.AddParameters(new { username = user.UserName, password = user.Password, token = user.Token, coins = user.Coins, elo = user.ELO, games_played = user.GamesPlayed, bio = user.BIO, image = user.Image });
        await command.ExecuteNonQueryAsync();

        return await connection.QuerySingleAsync<User>("SELECT * FROM users WHERE username = @username", new { username = user.UserName });
    }
    catch (Exception e)
    {
        logger.LogError(e, "Error while updating user");
        throw;
    }
}
    public async ValueTask<User?> GetByToken(string token)
    {
        try
        {
            await using DbConnection connection = CreateConnection();
            await connection.OpenAsync();
            User? user = await connection.QuerySingleAsync<User>("SELECT * FROM users WHERE token = @token", new { token });

            return user;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while getting user by token");
            throw;
        }
    }
}