namespace Tgs.GamePlayerBalance.Core;

public interface ISnowflake
{
    long GetDatacenterId();

    long GetWorkerId();

    long NextId();
}
