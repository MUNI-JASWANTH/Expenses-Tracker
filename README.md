# FirstMauiApp – Finance Tracker

A .NET MAUI personal finance tracker using MVVM, CommunityToolkit.Mvvm, and SQLite.

## Features

- Monthly dashboard with income, expenses, and net balance
- Add, edit, delete, and filter transactions
- Expense-category monthly budgets
- Monthly income and expense breakdowns
- Local SQLite persistence
- Light/dark theme resources

## Requirements

- .NET 11 SDK
- .NET MAUI workload for the target platform
- Android SDK/emulator for Android builds (or the relevant Apple/Windows workload for other targets)

## Build and run

From the project directory:

```bash
dotnet workload restore
dotnet restore FirstMauiApp.csproj
dotnet build FirstMauiApp.csproj -f net11.0-android
```

To deploy to a connected Android device or emulator, use Visual Studio or:

```bash
dotnet build FirstMauiApp.csproj -f net11.0-android -t:Run
```

The app stores its database in the platform-specific MAUI application data directory as `finance_tracker.db3`.

## Notes

- The project targets .NET 11 as configured in the supplied source project.
- SQLite's bundled native provider is initialized at application startup.
- Amounts use the device's current culture and currency formatting.
