using System.Numerics;

public static class MyMath
{
    public static bool IsBetween<T>(T min, T max, T value) where T : INumber<T> => value >= min && value < max;
}
