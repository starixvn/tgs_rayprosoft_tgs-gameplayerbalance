using MongoDB.Driver;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGamePlayerBalanceRepositories(
        this IServiceCollection services,
        Func<IServiceProvider, IMongoDatabase> mongoDatabaseResolver)
        => services
        .AddTransient<IBrandRepository, BrandRepository>()
        .Decorate<IBrandRepository>((innerRepository, sp) => ActivatorUtilities.CreateInstance<BrandCacheRepository>(sp, innerRepository))
        .AddTransient<IGameProviderRepository, GameProviderRepository>()
        .Decorate<IGameProviderRepository>((innerRepository, sp) => ActivatorUtilities.CreateInstance<GameProviderCacheRepository>(sp, innerRepository))
        .AddTransient<IBrandFormRepository>((sp) => ActivatorUtilities.CreateInstance<BrandFormRepository>(sp, mongoDatabaseResolver(sp)))
        .AddTransient<IImportFormRepository>((sp) => ActivatorUtilities.CreateInstance<ImportFormRepository>(sp, mongoDatabaseResolver(sp)))
        .AddTransient<IPlayerFormRepository>((sp) => ActivatorUtilities.CreateInstance<PlayerFormRepository>(sp, mongoDatabaseResolver(sp)));
}
