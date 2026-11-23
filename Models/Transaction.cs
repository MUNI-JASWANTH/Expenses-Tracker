using SQLite;

namespace FirstMauiApp.Models;

public enum TransactionType
{
    Income,
    Expense
}

public enum TransactionCategory
{
    // Income categories
    Salary,
    Freelance,
    Investment,
    Gift,
    OtherIncome,

    // Expense categories
    Food,
    Transport,
    Housing,
    Utilities,
    Entertainment,
    Healthcare,
    Shopping,
    Education,
    OtherExpense
}

[Table("Transactions")]
public class Transaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public TransactionType Type { get; set; }

    public TransactionCategory Category { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.Now;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Helper properties for UI
    public string TypeDisplay => Type.ToString();

    public string CategoryDisplay => Category.ToString();

    public string FormattedAmount => Amount.ToString("C");

    public string FormattedSignedAmount => $"{(Type == TransactionType.Income ? "+" : "-")}{Amount:C}";

    public Color CategoryColor => GetCategoryColor();

    public string CategoryIcon => GetCategoryIcon();

    private Color GetCategoryColor()
    {
        return Type switch
        {
            TransactionType.Income => Colors.Green,
            TransactionType.Expense => Colors.Red,
            _ => Colors.Gray
        };
    }

    private string GetCategoryIcon()
    {
        return Category switch
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
}