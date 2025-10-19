using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace LinkRouter.Settings.Avalonia.Converters;

public sealed class BooleanNotConverter : IValueConverter
{
    public static BooleanNotConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }

        return BindingOperations.DoNothing;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }

        return BindingOperations.DoNothing;
    }
}
