using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGamePlayerBalanceRepositories(this IServiceCollection services)
        => services
        .AddTransient<IBrandRepository, BrandRepository>()
        .Decorate<IBrandRepository>((innerRepository, sp) => ActivatorUtilities.CreateInstance<BrandCacheRepository>(sp, innerRepository))
        .AddTransient<IGameProviderRepository, GameProviderRepository>()
        .Decorate<IGameProviderRepository>((innerRepository, sp) => ActivatorUtilities.CreateInstance<GameProviderCacheRepository>(sp, innerRepository));
}
