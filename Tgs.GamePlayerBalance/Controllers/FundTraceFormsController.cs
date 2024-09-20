using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FundTraceFormsController : ControllerBase
{
	[HttpPost("BrandFormAdd")]
	public ValueTask BrandFormAddAsync(
		[FromServices] ICommandService<FundTraceBrandFormAddCommand> service,
		FundTraceBrandFormAddViewModel source)
		=> service.ExecuteAsync(
			command: new FundTraceBrandFormAddCommand(
				GameProviderCode: source.GameProviderCode,
				BrandCode: source.BrandCode,
				StartTime: source.StartTime,
				EndTime: source.EndTime),
			cancellationToken: HttpContext.RequestAborted);

	[HttpPost("GameProviderFormAdd")]
	public ValueTask FundTraceGameProviderFormAddAsync(
		[FromServices] ICommandService<FundTraceGameProviderFormAddCommand> service,
		FundTraceGameProviderFormAddViewModel source)
		=> service.ExecuteAsync(
			command: new FundTraceGameProviderFormAddCommand(
				GameProviderCode: source.GameProviderCode,
				StartTime: source.StartTime,
				EndTime: source.EndTime),
			cancellationToken: HttpContext.RequestAborted);
}
