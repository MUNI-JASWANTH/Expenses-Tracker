using System.Globalization;

namespace FirstMauiApp.Services;

/// <summary>
/// Controls display formatting only. Changing the selected currency never converts stored amounts.
/// </summary>
public static class CurrencySettings
{
    private const string PreferenceKey = "display_currency_v1";
    private static readonly Dictionary<string, (string Symbol, string Culture)> Supported = new()
    {
        ["INR"] = ("₹", "en-IN"),
        ["USD"] = ("$", "en-US"),
        ["EUR"] = ("€", "de-DE"),
        ["GBP"] = ("£", "en-GB"),
        ["JPY"] = ("¥", "ja-JP"),
        ["AUD"] = ("A$", "en-AU"),
        ["CAD"] = ("C$", "en-CA"),
        ["AED"] = ("AED ", "en-AE")
    };

    public static IReadOnlyList<string> Codes { get; } = Supported.Keys.ToArray();

    public static string SelectedCode
    {
        get
        {
            var code = Preferences.Default.Get(PreferenceKey, "INR");
            return Supported.ContainsKey(code) ? code : "INR";
        }
        set
        {
            if (!Supported.ContainsKey(value))
                throw new ArgumentOutOfRangeException(nameof(value), "Unsupported currency code.");
            Preferences.Default.Set(PreferenceKey, value);
        }
    }

    public static string Format(decimal amount)
    {
        var setting = Supported[SelectedCode];
        var culture = (CultureInfo)CultureInfo.GetCultureInfo(setting.Culture).Clone();
        culture.NumberFormat.CurrencySymbol = setting.Symbol;
        return amount.ToString("C", culture);
    }
}
