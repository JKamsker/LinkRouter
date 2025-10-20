using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LinkRouter.Settings.Converters;

public sealed class StringHasValueConverter : IValueConverter
{
    public static StringHasValueConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            return !string.IsNullOrWhiteSpace(text);
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
