using FirstMauiApp.Services;
using SQLite;

namespace FirstMauiApp.Models;

[Table("Budgets")]
public class Budget
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public TransactionCategory Category { get; set; }

    public decimal MonthlyLimit { get; set; }

    public int Month { get; set; } = DateTime.Now.Month;

    public int Year { get; set; } = DateTime.Now.Year;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Helper properties
    public string CategoryDisplay => Category.ToString();

    public string FormattedLimit => CurrencySettings.Format(MonthlyLimit);
}