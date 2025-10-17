using System;
using Microsoft.UI.Xaml.Data;

namespace LinkRouter.Settings.Converters;

public sealed class ByteSizeToStringConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is long longValue)
        {
            return FormatSize(longValue);
        }

        if (value is int intValue)
        {
            return FormatSize(intValue);
        }

        return null;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        double size = bytes;
        string[] units = ["KB", "MB", "GB", "TB"];
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.#} {units[unitIndex]}";
    }
}
