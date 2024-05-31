using Microsoft.EntityFrameworkCore;
using Tgs.GameFundTransfer.Client;
using Tgs.GamePlayerBalance;
using Tgs.GamePlayerBalance.Core;
using Tgs.GameProviders.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddGamePlayerBalance(
		this IServiceCollection services,
		Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptions,
		Action<MongoDBOptions, IServiceProvider> mongoDBconfigure,
		Action<IPAddressOptions, IServiceProvider> ipAddressOptions,
		Action<GameProvidersClientOptions, IServiceProvider> gameProvidersClientOptions,
		Action<GameFundTransferClientOptions, IServiceProvider> gameFundTransferClientOptions)
		=> services
		.AddOptions<MongoDBOptions>()
		.Configure(mongoDBconfigure)
		.Services
		.AddSingleton<MongoDBConnectionManager>()
		.AddGamePlayerBalanceCore(
			ipAddressOptions,
			gameProvidersClientOptions,
			gameFundTransferClientOptions)
		.AddGamePlayerBalanceRepositories(
			dbContextOptions: dbContextOptions,
			(sp) => sp.GetRequiredService<MongoDBConnectionManager>().MongoDatabase);
}
