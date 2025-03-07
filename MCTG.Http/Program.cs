using MCTG.Database;
using MCTG.Http;
using MCTG.Http.Handlers;
using MCTG.Repositories;
using MCTG.Repositories.Postgres;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureDatabase()
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<IUserRepository, NpgsqlUserRepository>();
        services.AddSingleton<ICardRepository, NpgsqlCardRepository>();
        services.AddSingleton<UserHandler>();
        services.AddSingleton<SessionHandler>();
        services.AddSingleton<PackageHandler>();
        services.AddSingleton<TransactionHandler>();
        services.AddSingleton<RequestExecutor>();
        services.AddHostedService<TCPListener>();
    }).Build();

var databaseConfig = host.Services.GetRequiredService<DatabaseConfig>();
await databaseConfig.ExecuteSqlScriptAsync("init.sql");

await host.RunAsync();