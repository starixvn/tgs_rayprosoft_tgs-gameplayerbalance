using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FundTraceGameProviderTimeFormsController : ControllerBase
{
	[HttpPost(Name = "FundTraceGameProviderTimeFormAdd")]
	public ValueTask FundTraceGameProviderTimeFormAddAsync(
		[FromServices] ICommandService<FundTraceGameProviderFormAddCommand> service,
		FundTraceGameProviderTimeFormAddViewModel source)
		=> service.ExecuteAsync(
			command: new FundTraceGameProviderFormAddCommand(
				GameProviderCode: source.GameProviderCode,
				StartTime: source.StartTime,
				EndTime: source.EndTime),
			cancellationToken: HttpContext.RequestAborted);
}
