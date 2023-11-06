using System.Net;

namespace Tgs.GamePlayerBalance.Core;

public interface ISnowflakeWork
{
    long GetDatacenterId();

    long GetId();

    IPAddress GetIPAddress();

    long GetWorkerId();
}
