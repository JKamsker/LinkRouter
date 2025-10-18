using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace LinkRouter.Settings.Avalonia.Converters;

public sealed class BooleanNegationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool flag)
        {
            return !flag;
        }

        return BindingOperations.DoNothing;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool flag)
        {
            return !flag;
        }

        return BindingOperations.DoNothing;
    }
}
