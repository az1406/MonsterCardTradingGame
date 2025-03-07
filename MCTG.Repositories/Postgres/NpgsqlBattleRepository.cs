using System.Data.Common;
using Microsoft.Extensions.Logging;
using MCTG.Database;
using MCTG.Models;
using Npgsql;

namespace MCTG.Repositories.Postgres
{
    public class NpgsqlBattleRepository : AbstractRepository, IBattleRepository
    {
        private readonly ILogger<NpgsqlBattleRepository> _logger;

        public NpgsqlBattleRepository(DatabaseConfig databaseConfig, ILogger<NpgsqlBattleRepository> logger)
            : base(databaseConfig)
        {
            _logger = logger;
        }

        public async Task<Battle> CreateBattle(Battle battle)
        {
            try
            {
                await using DbConnection connection = CreateConnection();
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = @"
            INSERT INTO battles (user1_token, user2_token, start_time, end_time, winner_token, is_draw, rounds, user1_wins, user2_wins)
            VALUES (@User1Token, @User2Token, @StartTime, @EndTime, @WinnerToken, @IsDraw, @Rounds, @User1Wins, @User2Wins)
            RETURNING id";
                command.Parameters.Add(new NpgsqlParameter("@User1Token", battle.User1Token));
                command.Parameters.Add(new NpgsqlParameter("@User2Token", battle.User2Token));
                command.Parameters.Add(new NpgsqlParameter("@StartTime", battle.StartTime));
                command.Parameters.Add(new NpgsqlParameter("@EndTime", battle.EndTime));
                command.Parameters.Add(new NpgsqlParameter("@WinnerToken", battle.WinnerToken));
                command.Parameters.Add(new NpgsqlParameter("@IsDraw", battle.IsDraw));
                command.Parameters.Add(new NpgsqlParameter("@Rounds", battle.Rounds));
                command.Parameters.Add(new NpgsqlParameter("@User1Wins", battle.User1Wins));
                command.Parameters.Add(new NpgsqlParameter("@User2Wins", battle.User2Wins));

                battle.Id = (int)await command.ExecuteScalarAsync();
                return battle;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while creating battle");
                throw;
            }
        }

        public async Task UpdateBattleResult(Battle battle)
        {
            try
            {
                await using DbConnection connection = CreateConnection();
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = @"
            UPDATE battles
            SET end_time = @EndTime, winner_token = @WinnerToken, is_draw = @IsDraw, rounds = @Rounds, user1_wins = @User1Wins, user2_wins = @User2Wins
            WHERE id = @Id";
                command.Parameters.Add(new NpgsqlParameter("@EndTime", battle.EndTime));
                command.Parameters.Add(new NpgsqlParameter("@WinnerToken", battle.WinnerToken));
                command.Parameters.Add(new NpgsqlParameter("@IsDraw", battle.IsDraw));
                command.Parameters.Add(new NpgsqlParameter("@Rounds", battle.Rounds));
                command.Parameters.Add(new NpgsqlParameter("@User1Wins", battle.User1Wins));
                command.Parameters.Add(new NpgsqlParameter("@User2Wins", battle.User2Wins));
                command.Parameters.Add(new NpgsqlParameter("@Id", battle.Id));

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while updating battle result");
                throw;
            }
        }
    }
}