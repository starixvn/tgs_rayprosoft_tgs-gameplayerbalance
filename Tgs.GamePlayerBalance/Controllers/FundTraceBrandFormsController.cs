using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FundTraceBrandFormsController : ControllerBase
{
	[HttpPost(Name = "FundTraceBrandFormAdd")]
	public ValueTask FundTraceBrandFormAddAsync(
		[FromServices] ICommandService<FundTraceBrandFormAddCommand> service,
		FundTraceBrandFormAddViewModel source)
		=> service.ExecuteAsync(
			command: new FundTraceBrandFormAddCommand(
				GameProviderCode: source.GameProviderCode,
				BrandCode: source.BrandCode,
				StartTime: null,
				EndTime: null),
			cancellationToken: HttpContext.RequestAborted);
}
