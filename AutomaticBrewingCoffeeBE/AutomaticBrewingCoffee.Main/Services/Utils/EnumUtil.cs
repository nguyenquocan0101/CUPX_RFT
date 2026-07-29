using System.ComponentModel;
using System.Reflection;

namespace AutomaticBrewingCoffee.Services.Utils;

public static class EnumUtil
{
    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public static string GetDescriptionFromEnum<T>(this T value)
    {
        FieldInfo field = value!.GetType().GetField(value.ToString()!)!;
        DescriptionAttribute attribute = field?.GetCustomAttribute<DescriptionAttribute>()!;

        return attribute?.Description ?? value.ToString();
    }

    public static IEnumerable<T> GetValues<T>()
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    public static T GetNextEnum<T>(T current)
    {
        var values = (T[])Enum.GetValues(typeof(T));

        int currentIndex = Array.IndexOf(values, current);

        int nextIndex = (currentIndex + 1) % values.Length;
        T nextValue = values[nextIndex];

        return nextValue;
    }
    
}