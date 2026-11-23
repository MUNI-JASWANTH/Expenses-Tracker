using Microsoft.Extensions.Logging;
using FirstMauiApp.Services;
using FirstMauiApp.ViewModels;
using FirstMauiApp.Views;

namespace FirstMauiApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		// Initialize the bundled native SQLite provider before opening any database connection.
		SQLitePCL.Batteries_V2.Init();

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Services
		builder.Services.AddSingleton<DatabaseService>();
		builder.Services.AddSingleton<BackupService>();

		// ViewModels
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<TransactionsViewModel>();
		builder.Services.AddTransient<AddTransactionViewModel>();
		builder.Services.AddTransient<BudgetsViewModel>();
		builder.Services.AddTransient<ReportsViewModel>();

		// Views
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<TransactionsPage>();
		builder.Services.AddTransient<AddTransactionPage>();
		builder.Services.AddTransient<BudgetsPage>();
		builder.Services.AddTransient<ReportsPage>();
		builder.Services.AddTransient<BackupPage>();

		return builder.Build();
	}
}
