using System.Data.Common;
using Npgsql;

namespace MCTG.Database;

public abstract class AbstractRepository(DatabaseConfig databaseConfig)
{
    protected DbConnection CreateConnection() => new NpgsqlConnection(databaseConfig.ConnectionString);
}