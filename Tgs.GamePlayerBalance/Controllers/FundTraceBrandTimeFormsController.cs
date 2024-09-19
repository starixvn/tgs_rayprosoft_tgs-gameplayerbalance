using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FundTraceBrandTimeFormsController : ControllerBase
{
	[HttpPost(Name = "FundTraceBrandTimeFormAdd")]
	public ValueTask FundTraceBrandTimeFormAddAsync(
		[FromServices] ICommandService<FundTraceBrandFormAddCommand> service,
		FundTraceBrandTimeFormAddViewModel source)
		=> service.ExecuteAsync(
			command: new FundTraceBrandFormAddCommand(
				GameProviderCode: source.GameProviderCode,
				BrandCode: source.BrandCode,
				StartTime: source.StartTime,
				EndTime: source.EndTime),
			cancellationToken: HttpContext.RequestAborted);
}
