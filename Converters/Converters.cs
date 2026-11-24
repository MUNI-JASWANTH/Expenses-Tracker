using FirstMauiApp.Services;
using System.Globalization;
using Microsoft.Maui.Controls;
using FirstMauiApp.Models;

namespace FirstMauiApp.Converters;

public class TransactionTypeToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TransactionType type)
        {
            return type == TransactionType.Income ? Colors.Green : Colors.Red;
        }
        return Colors.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter supports one-way bindings only.");
    }
}

public class TransactionTypeToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TransactionType type)
        {
            return type == TransactionType.Income ? "+" : "-";
        }
        return "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter supports one-way bindings only.");
    }
}

public class CategoryToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TransactionCategory category)
        {
            return category switch
            {
                TransactionCategory.Salary => "💼",
                TransactionCategory.Freelance => "💻",
                TransactionCategory.Investment => "📈",
                TransactionCategory.Gift => "🎁",
                TransactionCategory.OtherIncome => "💰",
                TransactionCategory.Food => "🍔",
                TransactionCategory.Transport => "🚌",
                TransactionCategory.Housing => "🏠",
                TransactionCategory.Utilities => "💡",
                TransactionCategory.Entertainment => "🎮",
                TransactionCategory.Healthcare => "🏥",
                TransactionCategory.Shopping => "🛍️",
                TransactionCategory.Education => "📚",
                TransactionCategory.OtherExpense => "📦",
                _ => "💸"
            };
        }
        return "💸";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter supports one-way bindings only.");
    }
}

public class BoolToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string param)
        {
            var parts = param.Split('|');
            if (parts.Length == 2)
            {
                return boolValue ? parts[0] : parts[1];
            }
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter supports one-way bindings only.");
    }
}

/// <summary>
/// Static singleton version of BoolToTextConverter for use with x:Static in XAML.
/// </summary>
public class BoolToTextConverterStatic : IValueConverter
{
    public static readonly BoolToTextConverterStatic Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string param)
        {
            var parts = param.Split('|');
            if (parts.Length == 2)
            {
                return boolValue ? parts[0] : parts[1];
            }
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter supports one-way bindings only.");
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter supports one-way bindings only.");
    }
}

public class DecimalToCurrencyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            var prefix = parameter?.ToString() ?? "";
            return $"{prefix}{CurrencySettings.Format(decimalValue)}";
        }
        return CurrencySettings.Format(0m);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter supports one-way bindings only.");
    }
}

public class CategoryToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TransactionCategory category)
        {
            return category switch
            {
                TransactionCategory.Food => Color.FromArgb("#FF6B6B"),
                TransactionCategory.Transport => Color.FromArgb("#4ECDC4"),
                TransactionCategory.Housing => Color.FromArgb("#45B7D1"),
                TransactionCategory.Utilities => Color.FromArgb("#FFA07A"),
                TransactionCategory.Entertainment => Color.FromArgb("#98D8C8"),
                TransactionCategory.Healthcare => Color.FromArgb("#F7DC6F"),
                TransactionCategory.Shopping => Color.FromArgb("#BB8FCE"),
                TransactionCategory.Education => Color.FromArgb("#85C1E9"),
                TransactionCategory.Salary => Color.FromArgb("#82E0AA"),
                TransactionCategory.Freelance => Color.FromArgb("#F8B500"),
                TransactionCategory.Investment => Color.FromArgb("#00CED1"),
                TransactionCategory.Gift => Color.FromArgb("#FF69B4"),
                _ => Color.FromArgb("#95A5A6")
            };
        }
        return Colors.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter supports one-way bindings only.");
    }
}

public class BudgetProgressColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double progress)
        {
            if (progress >= 1.0) return Colors.Red;
            if (progress >= 0.8) return Colors.Orange;
            if (progress >= 0.5) return Colors.Yellow;
            return Colors.Green;
        }
        return Colors.Green;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter supports one-way bindings only.");
    }
}

public class EmptyListToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is System.Collections.IEnumerable enumerable)
        {
            return !enumerable.Cast<object>().Any();
        }
        return true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter supports one-way bindings only.");
    }
}

public class NegativeToPositiveConverter : IValueConverter
{
    // Selects colors based on balance sign.
    // ConverterParameter must contain "positiveColor|negativeColor".
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not decimal balance || parameter is not string colors)
            return Colors.Gray;

        var parts = colors.Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
            return Colors.Gray;

        var selected = balance >= 0 ? parts[0] : parts[1];
        try
        {
            return Color.FromArgb(selected);
        }
        catch (FormatException)
        {
            return Colors.Gray;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException("Balance color conversion is one-way.");
}
