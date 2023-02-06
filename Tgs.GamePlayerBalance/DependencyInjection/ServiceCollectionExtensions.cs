using Tgs.GameCommand.Client;
using Tgs.GameFundTransfer.Client;
using Tgs.GameProviders.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGamePlayerBalance(
        this IServiceCollection services,
        Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
        Action<GameProvidersClientOptions, IServiceProvider> gameProvidersClientOptions,
        Action<GameFundTransferClientOptions, IServiceProvider> gameFundTransferClientOptions)
        => services
            .AddMemoryCache()
            .AddGamePlayerBalanceCore(
                gameCommandClientOptions,
                gameProvidersClientOptions,
                gameFundTransferClientOptions)
            .AddGamePlayerBalanceRepositories();
}
