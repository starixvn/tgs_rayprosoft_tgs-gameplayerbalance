using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Entities;
using Tgs.GamePlayerBalance.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGamePlayerBalanceRepositories(
		this IServiceCollection services,
		Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptions,
		Func<IServiceProvider, IMongoDatabase> mongoDatabaseResolver)
		=> services
		.AddDbContext<TgsContext>(dbContextOptions)
		.AddTransient<IBrandRepository, BrandRepository>()
		.AddTransient<IGameProviderRepository, GameProviderRepository>()
		.AddTransient<IPlayerRepository, PlayerRepository>()
		.AddTransient<IGameFundTransferTraceRepository, GameFundTransferTraceRepository>()
		.AddTransient<IBrandFormRepository>((sp) => ActivatorUtilities.CreateInstance<BrandFormRepository>(sp, mongoDatabaseResolver(sp)))
		.AddTransient<IImportFormRepository>((sp) => ActivatorUtilities.CreateInstance<ImportFormRepository>(sp, mongoDatabaseResolver(sp)))
		.AddTransient<IPlayerFormRepository>((sp) => ActivatorUtilities.CreateInstance<PlayerFormRepository>(sp, mongoDatabaseResolver(sp)));
}
