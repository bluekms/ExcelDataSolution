namespace Analyzers.Sample;

public class Spaceship
{
    public static void SetSpeed(long speed)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(speed, 299_792_458);
    }
}
