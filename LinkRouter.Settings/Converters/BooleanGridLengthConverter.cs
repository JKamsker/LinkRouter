using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace LinkRouter.Settings.Converters;

public sealed class BooleanGridLengthConverter : IValueConverter
{
    public GridLength TrueLength { get; set; } = GridLength.Auto;

    public GridLength FalseLength { get; set; } = GridLength.Auto;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? TrueLength : FalseLength;
        }

        return BindingOperations.DoNothing;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("BooleanGridLengthConverter only supports one-way conversion.");
    }
}
