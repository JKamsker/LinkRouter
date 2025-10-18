using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LinkRouter.Settings.Avalonia.Converters;

public sealed class StringToBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string text && !string.IsNullOrWhiteSpace(text);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}
