using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MCTG.Database;

public static class DatabaseConfigurationExtensions
{
    public static IHostBuilder ConfigureDatabase(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            DatabaseConfig databaseConfig = new() { ConnectionString = context.Configuration.GetConnectionString("PostgresConnection")! };
            services.AddSingleton(databaseConfig);
        });

        return hostBuilder;
    }
}