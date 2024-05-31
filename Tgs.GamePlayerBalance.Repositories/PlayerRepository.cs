using Microsoft.EntityFrameworkCore;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Entities;

namespace Tgs.GamePlayerBalance.Repositories;
internal class PlayerRepository(
	TgsContext tgsContext) : IPlayerRepository
{
	public async ValueTask<Core.Models.PlayerInfo?> GetAsync(string playerCode, CancellationToken cancellationToken = default)
		=> await tgsContext.Players
		.Where(p => p.PlayerCode == playerCode)
		.Join(
			tgsContext.Brands,
			player => player.BrandId,
			brand => brand.Id,
			(player, brand) => new Core.Models.PlayerInfo(
				brand.Code,
				player.PlayerCode,
				player.Username,
				player.DefaultWalletId,
				player.DefaultWalletCurrencyCode))
		.FirstOrDefaultAsync(
			cancellationToken: cancellationToken)
		.ConfigureAwait(false);
	public IAsyncEnumerable<Core.Models.PlayerInfo> QueryAsync(Guid brandId, int pageIndex = 1, int pageSize = 500, CancellationToken cancellationToken = default)
	   => (from player in tgsContext.Players.AsQueryable()
		   join brand in tgsContext.Brands.AsQueryable()
		   on player.BrandId equals brand.Id
		   where player.BrandId == brandId
		   orderby player.PlayerCode
		   select new Core.Models.PlayerInfo(
			   brand.Code,
			   player.PlayerCode,
			   player.Username,
			   player.DefaultWalletId,
			   player.DefaultWalletCurrencyCode
			   ))
		.Skip(pageSize * (pageIndex - 1))
		.Take(pageSize)
		.ToAsyncEnumerable();

}
