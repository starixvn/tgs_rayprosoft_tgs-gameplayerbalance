using Microsoft.AspNetCore.Mvc;
using Tgs.GamePlayerBalance.Core;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("[controller]")]
[ApiController]
public class SnowflakeController : ControllerBase
{
    [HttpGet("worker")]
    public long GetWorkerId(
        [FromServices] ISnowflake repository)
        => repository.GetWorkerId();

    [HttpGet("datacenter")]
    public long GetDatacenterId(
        [FromServices] ISnowflake repository)
        => repository.GetDatacenterId();

    [HttpGet("next")]
    public long NextId(
        [FromServices] ISnowflake repository)
        => repository.NextId();
}
