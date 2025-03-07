using System.IO;
using System.Threading.Tasks;
using Npgsql;

namespace MCTG.Database
{
    public class DatabaseConfig
    {
        public string ConnectionString { get; set; } = string.Empty;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new InvalidOperationException("Connection string is required");
            }
        }

        public async Task ExecuteSqlScriptAsync(string scriptPath)
        {
            var script = await File.ReadAllTextAsync(scriptPath);
            await using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(script, connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}