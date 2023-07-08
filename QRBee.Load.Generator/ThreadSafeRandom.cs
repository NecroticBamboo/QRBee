namespace QRBee.Load.Generator;

/// <summary>
/// https://stackoverflow.com/questions/3049467/is-c-sharp-random-number-generator-thread-safe
/// </summary>
public class ThreadSafeRandom
{
    private static readonly Random _global = new Random();
    [ThreadStatic] private static Random? _local;

    public int Next()
    {
        Init();
        return _local!.Next();
    }

    public double NextDouble()
    {
        Init();
        return _local!.NextDouble();
    }

    public int NextInRange(int start, int end)
    {
        var n = Next();
        return start + n % (end - start);
    }

    public double NextDoubleInRange(double min, double max)
    {
        var n = NextDouble();
        return min + (max - min) * n;
    }

    private static void Init()
    {
        if (_local == null)
        {
            int seed;
            lock (_global)
            {
                seed = _global.Next();
            }
            _local = new Random(seed);
        }
    }
}