# Display currency settings

Open the **Settings** tab and select INR, USD, EUR, GBP, JPY, AUD, CAD, or AED.
The selected currency code is saved using MAUI Preferences on the current device. Dashboard, Transactions, Budgets, and Reports use the selected currency when those tabs are reopened/refreshed.

**Important:** This is display formatting, not foreign-exchange conversion. Existing stored amounts remain unchanged, and the app still assumes transactions and budgets are recorded in one consistent currency. Do not mix denominations within the same database. Backups contain the numeric records; the currency preference is device-local and is not currently embedded in the backup format.

## Suggested verification in Visual Studio
1. Rebuild the solution on Windows.
2. Open Settings, select INR, verify the example displays a rupee symbol.
3. Open Dashboard, Transactions, Budgets, and Reports; check amount labels.
4. Switch to USD; revisit tabs and verify their amount labels update.
5. Close and restart the app; confirm the chosen setting persists.
6. Export a backup, change the display currency, restore; verify amounts do not change.
7. Test on Android, iOS, and Mac Catalyst on the corresponding build devices before release.

This package has been checked for ZIP integrity and XAML well-formedness, but was not compiled or runtime-tested in this environment.
