using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Snowflake.Core;

namespace Tgs.GamePlayerBalance.Core;

internal class SnowflakeUtensil : ISnowflake
{
    private readonly IdWorker _idWorker;
    private readonly ILogger<SnowflakeUtensil> _logger;

    public SnowflakeUtensil(
        ILogger<SnowflakeUtensil> logger,
        IOptions<IPAddressOptions> options)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var ipAddress = IPAddress.Parse(options.Value.IPAddress);

        var address = ipAddress.IsIPv6Teredo
            ? (long)BitConverter.ToUInt64(IPAddress.Parse(options.Value.IPAddress).GetAddressBytes())
            : BitConverter.ToUInt32(IPAddress.Parse(options.Value.IPAddress).GetAddressBytes());

        var workerId = (address % 255 & 30) + 1;
        var datacenterId = ((address >> 5) % 255 & 30) + 1;
        _logger.LogInformation("IpAddress:{ipAddress} | Address:{address} | WorkerId:{workerId} | DatacenterId:{datacenterId}",
            ipAddress,
            address,
            workerId,
            datacenterId);
        _idWorker = new IdWorker(workerId, datacenterId);
    }

    public long GetDatacenterId()
        => _idWorker.DatacenterId;

    public long GetWorkerId()
        => _idWorker.WorkerId;

    public long NextId()
        => _idWorker.NextId();
}
