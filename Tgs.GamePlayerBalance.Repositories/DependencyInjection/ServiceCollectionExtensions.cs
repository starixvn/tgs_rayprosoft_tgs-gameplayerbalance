using Tgs.GamePlayerBalance.Core;
using Tgs.GameProviders.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGamePlayerBalanceRepositories(this IServiceCollection services)
        => services
        .AddTransient<IGameProviderRepository, GameProviderRepository>()
        .Decorate<IGameProviderRepository>((innerRepository, sp) => ActivatorUtilities.CreateInstance<GameProviderCacheRepository>(sp, innerRepository));
}
