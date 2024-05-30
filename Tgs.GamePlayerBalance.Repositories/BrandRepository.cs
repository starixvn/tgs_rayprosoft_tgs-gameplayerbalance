using Tgs.GameCommand.Client;
using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.Repositories;

internal class BrandRepository(
	IGameCommandClient client) : IBrandRepository
{
	public async ValueTask<Guid> GetIdAsync(
		string code,
		CancellationToken cancellationToken = default)
		=> await client.BrandGetBrandIdAsync(
			code,
			cancellationToken)
		.ConfigureAwait(false)
		?? throw new Exception($"The brand id does not exist.({code})");
}
