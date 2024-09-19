using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FundTraceGameProviderFormsController : ControllerBase
{
	[HttpPost(Name = "FundTraceGameProviderFormAdd")]
	public ValueTask FundTraceGameProviderFormAddAsync(
		[FromServices] ICommandService<FundTraceGameProviderFormAddCommand> service,
		FundTraceGameProviderFormAddViewModel source)
		=> service.ExecuteAsync(
			command: new FundTraceGameProviderFormAddCommand(
				GameProviderCode: source.GameProviderCode,
				StartTime: null,
				EndTime: null),
			cancellationToken: HttpContext.RequestAborted);
}
