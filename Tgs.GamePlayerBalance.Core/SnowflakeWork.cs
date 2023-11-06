using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Snowflake.Core;

namespace Tgs.GamePlayerBalance.Core;

internal class SnowflakeWork : ISnowflakeWork
{
    private readonly IdWorker _idWorker;
    private readonly IPAddress _iPAddress;
    private readonly ILogger<SnowflakeWork> _logger;

    public SnowflakeWork(
        ILogger<SnowflakeWork> logger,
        IOptions<IPAddressOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _iPAddress = IPAddress.Parse(options.Value.IPAddress);
        var address = _iPAddress.IsIPv6Teredo
            ? (long)BitConverter.ToUInt64(IPAddress.Parse(options.Value.IPAddress).GetAddressBytes())
            : BitConverter.ToUInt32(IPAddress.Parse(options.Value.IPAddress).GetAddressBytes());
        var workerId = (address % 255 & 30) + 1;
        var datacenterId = ((address >> 5) % 255 & 30) + 1;
        _idWorker = new IdWorker(workerId, datacenterId);
        _logger.LogInformation("IpAddress:{ipAddress} | Address:{address} | WorkerId:{workerId} | DatacenterId:{datacenterId}",
            _iPAddress,
            address,
            workerId,
            datacenterId);
    }

    public long GetDatacenterId()
        => _idWorker.DatacenterId;

    public long GetId()
        => _idWorker.NextId();

    public IPAddress GetIPAddress()
        => _iPAddress;

    public long GetWorkerId()
        => _idWorker.WorkerId;
}
