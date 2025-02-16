// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Analyzers.Sample;

// If you don't see warnings, build the Analyzers Project.
public class Examples
{
    public class MyCompanyClass // Try to apply quick fix using the IDE.
    {
    }

    public static void ToStars()
    {
        Spaceship.SetSpeed(300000000); // Invalid value, it should be highlighted.
        Spaceship.SetSpeed(42);
    }
}
