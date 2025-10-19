using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LinkRouter.Settings.Avalonia.Converters;

public sealed class NullableToBooleanConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hasValue = value switch
        {
            string s => !string.IsNullOrWhiteSpace(s),
            Array array => array.Length > 0,
            System.Collections.IEnumerable enumerable => HasAny(enumerable),
            null => false,
            _ => true
        };

        return Invert ? !hasValue : hasValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();

    private static bool HasAny(System.Collections.IEnumerable enumerable)
    {
        var enumerator = enumerable.GetEnumerator();
        try
        {
            return enumerator.MoveNext();
        }
        finally
        {
            if (enumerator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
