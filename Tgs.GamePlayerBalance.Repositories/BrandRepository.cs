using Microsoft.EntityFrameworkCore;
using Tgs.GamePlayerBalance.Core;
using Tgs.GamePlayerBalance.Entities;

namespace Tgs.GamePlayerBalance.Repositories;

internal class BrandRepository(
	TgsContext tgsContext) : IBrandRepository
{
	public async ValueTask<Core.Models.BrandInfo> GetAsync(string code, CancellationToken cancellationToken = default)
		=> await tgsContext.Brands
		.Where(b => b.Code == code)
		.Select(b => new Core.Models.BrandInfo(
			b.Id,
			b.Code))
		.FirstOrDefaultAsync(
			cancellationToken: cancellationToken)
		.ConfigureAwait(false)
		?? throw new ArgumentNullException(code, "The brand id does not exist.");
}
