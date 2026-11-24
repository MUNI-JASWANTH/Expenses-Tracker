using FirstMauiApp.Views;

namespace FirstMauiApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Tab pages already have routes declared in AppShell.xaml.
		Routing.RegisterRoute(nameof(AddTransactionPage), typeof(AddTransactionPage));
	}
}
