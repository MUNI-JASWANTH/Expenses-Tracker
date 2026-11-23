using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls;
using FirstMauiApp.Models;

namespace FirstMauiApp.Converters;

public class CategoryListConverter : IValueConverter
{
    public static readonly CategoryListConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TransactionType type)
        {
            if (type == TransactionType.Income)
            {
                return new List<TransactionCategory>
                {
                    TransactionCategory.Salary,
                    TransactionCategory.Freelance,
                    TransactionCategory.Investment,
                    TransactionCategory.Gift,
                    TransactionCategory.OtherIncome
                };
            }
            else
            {
                return new List<TransactionCategory>
                {
                    TransactionCategory.Food,
                    TransactionCategory.Transport,
                    TransactionCategory.Housing,
                    TransactionCategory.Utilities,
                    TransactionCategory.Entertainment,
                    TransactionCategory.Healthcare,
                    TransactionCategory.Shopping,
                    TransactionCategory.Education,
                    TransactionCategory.OtherExpense
                };
            }
        }
        return new List<TransactionCategory>();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter is one-way.");
    }
}

public class TransactionTypeToCategoryColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TransactionType type || parameter is not string param)
            return Colors.Gray;

        // Button parameters are a single type name; selected buttons are colored by type.
        if (Enum.TryParse<TransactionType>(param, true, out var buttonType))
            return type == buttonType
                ? (type == TransactionType.Income ? Color.FromArgb("#2E7D32") : Color.FromArgb("#C62828"))
                : Color.FromArgb("#9E9E9E");

        // Entry parameter is a pair of colors, income first and expense second.
        var parts = param.Split('|');
        if (parts.Length == 2)
        {
            try { return Color.FromArgb(type == TransactionType.Income ? parts[0] : parts[1]); }
            catch (FormatException) { return Colors.Gray; }
        }

        return Colors.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter is one-way.");
    }
}