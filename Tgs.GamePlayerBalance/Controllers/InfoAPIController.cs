using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Tgs.GamePlayerBalance.Controllers;

[Route("api/info")]
[ApiController]
public class InfoAPIController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public InfoAPIController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Gets the asynchronous.
    /// </summary>
    /// <param name="_hostingEnvironment">The hosting environment.</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<object> GetAsync([FromServices] IWebHostEnvironment _hostingEnvironment)
    {
        var hostName = Dns.GetHostName();
        var hostEntry = await Dns.GetHostEntryAsync(hostName);
        var hostIp = Array.Find(hostEntry.AddressList,
            x => x.AddressFamily == AddressFamily.InterNetwork);
        return new
        {
            Data = new
            {
                Environment.MachineName,
                HostName = hostName,
                HostIp = hostIp?.ToString() ?? string.Empty,
                Environment = _hostingEnvironment.EnvironmentName,
                OsVersion = $"{Environment.OSVersion}",
                Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                ProcessCount = Environment.ProcessorCount,
                POD_IP = _configuration.GetValue<string>("POD_IP")
            }
        };
    }
}
