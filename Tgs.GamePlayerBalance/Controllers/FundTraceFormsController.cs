using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core.ApplicationServices;
using Tgs.GamePlayerBalance.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FundTraceFormsController : ControllerBase
{
	[HttpPost(Name = "FundTraceFormAdd")]
	public ValueTask FundTraceFormAddAsync(
		[FromServices] ICommandService<FundTraceFormAddCommand> service,
		FundTraceFormAddViewModel source)
		=> service.ExecuteAsync(
			command: new FundTraceFormAddCommand(
				GameProviderCode: source.GameProviderCode,
				BrandCode: source.BrandCode,
				StartTime: source.StartTime,
				EndTime: source.EndTime),
			cancellationToken: HttpContext.RequestAborted);
}
