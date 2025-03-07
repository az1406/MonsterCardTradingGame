using System.Data.Common;
using Microsoft.Extensions.Logging;
using MCTG.Database;
using MCTG.Models;
using Npgsql;

namespace MCTG.Repositories.Postgres;

public class NpgsqlCardRepository : AbstractRepository, ICardRepository
{
    private readonly ILogger<NpgsqlCardRepository> _logger;

    public NpgsqlCardRepository(DatabaseConfig databaseConfig, ILogger<NpgsqlCardRepository> logger)
        : base(databaseConfig)
    {
        _logger = logger;
    }

    public async Task<Card?> GetById(string id)
    {
        try
        {
            await using DbConnection connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM cards WHERE id = @id";
            command.Parameters.Add(new NpgsqlParameter("@id", id));

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Card
                {
                    Id = reader.GetString(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    ElementType = reader.GetString(reader.GetOrdinal("element_type")),
                    PackageNumber = reader.GetInt32(reader.GetOrdinal("package_number")),
                    IsSpell = reader.GetBoolean(reader.GetOrdinal("is_spell")),
                    Damage = reader.GetDouble(reader.GetOrdinal("damage"))
                };
            }
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting card by ID");
            throw;
        }
    }

    public async Task Create(Card card)
    {
        try
        {
            await using DbConnection connection = CreateConnection();
            await connection.OpenAsync();

            // Check if card already exists
            if (await GetById(card.Id) != null)
            {
                throw new InvalidOperationException("Card with the same ID already exists");
            }

            await using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO cards (id, name, element_type, package_number, is_spell, damage) VALUES (@Id, @Name, @ElementType, @PackageNumber, @IsSpell, @Damage)";
            command.Parameters.Add(new NpgsqlParameter("@Id", card.Id));
            command.Parameters.Add(new NpgsqlParameter("@Name", card.Name));
            command.Parameters.Add(new NpgsqlParameter("@ElementType", card.ElementType));
            command.Parameters.Add(new NpgsqlParameter("@PackageNumber", card.PackageNumber));
            command.Parameters.Add(new NpgsqlParameter("@IsSpell", card.IsSpell));
            command.Parameters.Add(new NpgsqlParameter("@Damage", card.Damage));
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while creating card");
            throw;
        }
    }

    public async Task Update(Card card)
    {
        try
        {
            await using DbConnection connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "UPDATE cards SET name = @Name, element_type = @ElementType, package_number = @PackageNumber, is_spell = @IsSpell, damage = @Damage WHERE id = @Id";
            command.Parameters.Add(new NpgsqlParameter("@Id", card.Id));
            command.Parameters.Add(new NpgsqlParameter("@Name", card.Name));
            command.Parameters.Add(new NpgsqlParameter("@ElementType", card.ElementType));
            command.Parameters.Add(new NpgsqlParameter("@PackageNumber", card.PackageNumber));
            command.Parameters.Add(new NpgsqlParameter("@IsSpell", card.IsSpell));
            command.Parameters.Add(new NpgsqlParameter("@Damage", card.Damage));
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while updating card");
            throw;
        }
    }

    public async Task<int> GetNextPackageNumber()
    {
        try
        {
            await using DbConnection connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COALESCE(MAX(package_number), 0) + 1 FROM cards";
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting next package number");
            throw;
        }
    }

    public async Task<List<Card>> GetCardsByPackageNumber(int packageNumber)
    {
        try
        {
            await using DbConnection connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM cards WHERE package_number = @packageNumber";
            command.Parameters.Add(new NpgsqlParameter("@packageNumber", packageNumber));

            var cards = new List<Card>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cards.Add(new Card
                {
                    Id = reader.GetString(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    ElementType = reader.GetString(reader.GetOrdinal("element_type")),
                    PackageNumber = reader.GetInt32(reader.GetOrdinal("package_number")),
                    IsSpell = reader.GetBoolean(reader.GetOrdinal("is_spell")),
                    Damage = reader.GetDouble(reader.GetOrdinal("damage"))
                });
            }

            _logger.LogInformation($"Retrieved {cards.Count} cards for package number {packageNumber}");
            return cards;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting cards by package number");
            throw;
        }
    }

    public async Task SaveCardToStack(string userToken, string cardId, int packageNumber)
    {
        try
        {
            await using DbConnection connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO stack (user_token, card_id, package_number) VALUES (@UserToken, @CardId, @PackageNumber)";
            command.Parameters.Add(new NpgsqlParameter("@UserToken", userToken));
            command.Parameters.Add(new NpgsqlParameter("@CardId", cardId));
            command.Parameters.Add(new NpgsqlParameter("@PackageNumber", packageNumber));
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while saving card to stack");
            throw;
        }
    }

    public async Task<int> GetCurrentPackageNumber()
    {
        try
        {
            await using DbConnection connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COALESCE(MAX(package_number), 0) + 1 FROM stack";
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting current package number");
            throw;
        }
    }

    public async Task<List<Card>> GetDeck(string userToken)
    {
        try
        {
            await using DbConnection connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT c.* FROM deck d JOIN cards c ON d.card_id = c.id WHERE d.user_token = @userToken";
            command.Parameters.Add(new NpgsqlParameter("@userToken", userToken));

            var cards = new List<Card>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cards.Add(new Card
                {
                    Id = reader.GetString(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("name")),
                    ElementType = reader.GetString(reader.GetOrdinal("element_type")),
                    PackageNumber = reader.GetInt32(reader.GetOrdinal("package_number")),
                    IsSpell = reader.GetBoolean(reader.GetOrdinal("is_spell")),
                    Damage = reader.GetDouble(reader.GetOrdinal("damage"))
                });
            }

            return cards;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting deck");
            throw;
        }
    }

    public async Task SaveCardToDeck(string userToken, string cardId)
    {
        try
        {
            await using DbConnection connection = CreateConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO deck (user_token, card_id) VALUES (@UserToken, @CardId)";
            command.Parameters.Add(new NpgsqlParameter("@UserToken", userToken));
            command.Parameters.Add(new NpgsqlParameter("@CardId", cardId));
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while saving card to deck");
            throw;
        }
    }
}