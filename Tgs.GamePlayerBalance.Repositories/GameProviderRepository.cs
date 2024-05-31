using Microsoft.EntityFrameworkCore;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Entities;

namespace Tgs.GamePlayerBalance.Repositories;

internal class GameProviderRepository(
	TgsContext tgsContext) : IGameProviderRepository
{
	public async ValueTask<Core.Models.GameProviderInfo> GetAsync(string code, CancellationToken cancellationToken = default)
	{
		code = code.ToUpper();
		return await tgsContext.GameProviders
			.Where(gp => gp.Code == code)
			.Select(gp => new Core.Models.GameProviderInfo(
				gp.Id,
				gp.Code))
			.FirstOrDefaultAsync(
			cancellationToken: cancellationToken)
			.ConfigureAwait(false)
			?? throw new ArgumentNullException(code, "The brand id does not exist.");
	}
}
