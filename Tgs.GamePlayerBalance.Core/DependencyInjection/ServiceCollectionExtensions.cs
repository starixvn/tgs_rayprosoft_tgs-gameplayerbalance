using System.Reflection;
using Tgs.GameCommand.Client;
using Tgs.GameFundTransfer.Client;
using Tgs.GamePlayerBalance.Core;
using Tgs.GameProviders.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGamePlayerBalanceCore(
        this IServiceCollection services,
         Action<IPAddressOptions, IServiceProvider> ipAddressOptions,
        Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
        Action<GameProvidersClientOptions, IServiceProvider> gameProvidersClientOptions,
        Action<GameFundTransferClientOptions, IServiceProvider> gameFundTransferClientOptions)
        => services
            .AddOptions<IPAddressOptions>()
            .Configure(ipAddressOptions)
            .Services
            .RegisterAllServiceUtilityTypes(
                Assembly.GetExecutingAssembly(),
                ServiceLifetime.Transient)
            .AddGameCommandClient(gameCommandClientOptions)
            .AddGameProvidersClient(gameProvidersClientOptions)
            .AddGameFundTransferClient(gameFundTransferClientOptions)
            .AddSingleton<ISnowflakeWork, SnowflakeWork>();
}
