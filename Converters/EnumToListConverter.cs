using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace FirstMauiApp.Converters;

public class EnumToListConverter : IValueConverter
{
    public static readonly EnumToListConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return new List<object>();

        var enumType = value.GetType();
        if (!enumType.IsEnum)
        {
            // If it's an enum value, get its type
            enumType = value.GetType();
        }

        return Enum.GetValues(enumType).Cast<object>().ToList();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter supports one-way bindings only.");
    }
}

public class EnumToListConverter<T> : IValueConverter where T : Enum
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter supports one-way bindings only.");
    }
}