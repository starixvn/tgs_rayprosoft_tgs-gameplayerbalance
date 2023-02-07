using Tgs.GameCommand.Client;
using Tgs.GameFundTransfer.Client;
using Tgs.GamePlayerBalance;
using Tgs.GamePlayerBalance.Core;
using Tgs.GameProviders.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGamePlayerBalance(
        this IServiceCollection services,
        Action<MongoDBOptions, IServiceProvider> mongoDBconfigure,
        Action<IPAddressOptions, IServiceProvider> ipAddressOptions,
        Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
        Action<GameProvidersClientOptions, IServiceProvider> gameProvidersClientOptions,
        Action<GameFundTransferClientOptions, IServiceProvider> gameFundTransferClientOptions)
        => services
            .AddOptions<MongoDBOptions>()
            .Configure(mongoDBconfigure)
            .Services
            .AddSingleton<MongoDBConnectionManager>()
            .AddMemoryCache()
            .AddGamePlayerBalanceCore(
                ipAddressOptions,
                gameCommandClientOptions,
                gameProvidersClientOptions,
                gameFundTransferClientOptions)
            .AddGamePlayerBalanceRepositories(
                (sp) => sp.GetRequiredService<MongoDBConnectionManager>().Database);
}
