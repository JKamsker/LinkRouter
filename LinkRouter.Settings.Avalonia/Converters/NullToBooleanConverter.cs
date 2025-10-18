using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace LinkRouter.Settings.Avalonia.Converters;

public sealed class NullToBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
