# Edit Transaction — update guide

The existing EditTransactionCommand, AddTransactionPage edit mode, and SQLite update path were retained. The Transactions list now includes visible **Edit** and **Delete** buttons in addition to swipe actions, making editing discoverable on Windows desktops and touch devices.

## Manual acceptance tests (required before release)

1. Add a transaction, open Transactions, and press **Edit**. Verify amount, type, category, date, time, and description are prefilled.
2. Change the amount and description, save, and confirm the transaction updates **without creating a duplicate**.
3. Change an expense to income and select a valid income category. Confirm Dashboard and Reports totals change when revisiting those pages.
4. Press Cancel; verify nothing changed in SQLite.
5. Try empty description, zero/negative amount, and invalid input. Verify the transaction is not saved.
6. Test Delete and cancel the confirmation; verify the record remains. Confirm deletion; verify it disappears.
7. Restart the app and confirm edits persist. Export a backup and restore it, verifying edited data is preserved.
8. Verify search, date filtering, and selected currency display after an edit.
9. Test layout with a narrow Windows window and on Android; test iOS/Mac Catalyst on a Mac build environment.

## Verification limits

ZIP integrity and XML parsing were checked in this environment. Compilation and runtime tests were **not** performed because the .NET MAUI toolchain is not available here. Rebuild and test in Visual Studio before distributing the app.
