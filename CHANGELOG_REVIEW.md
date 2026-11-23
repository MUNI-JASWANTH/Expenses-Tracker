# Monthly Expense Tracker — changes and verification

## Added
- Live transaction search by description, category, or income/expense type. Search combines with the selected date range and existing filters; totals update with visible results.

## Fixed / hardened
- Editing transactions now saves a detached copy, avoiding mutation of the displayed item before database persistence succeeds.
- Delete errors are surfaced to the user.
- CSV export uses invariant numeric formatting and escapes formula-like description prefixes.
- Reject invalid transaction dates and budget years.
- Display name changed to Monthly Expense Tracker.

## Limitations and required tests
- This environment does not include the .NET SDK, MAUI workloads, Apple SDK, or emulators; no platform builds or runtime tests were executed. A successful Windows build, Android emulator test, and Mac-hosted iOS/Mac Catalyst test are required before claiming cross-platform readiness.
- Existing stored data is retained by the unchanged application identifier and SQLite database filename. Back up your app data before replacing an installed build.
- Monetary display follows the device locale. It does not implement currency conversion or multi-currency accounting.
- Search currently searches the date-range-loaded transactions, not all history outside that range.

## Manual acceptance tests
1. Create income and expense; verify dashboard and reports totals.
2. Search by partial description/category/type; verify visible rows and filtered totals.
3. Clear search and filters; verify all in-range rows return.
4. Edit an existing transaction; verify amount, category and persistence after restart.
5. Delete a transaction; verify totals and persistence after restart.
6. Set, edit and remove a monthly category budget; verify month navigation.
7. Export CSV with commas, quotes and leading equals signs in descriptions; inspect with a text editor.
8. Test on Windows, Android, iOS and Mac Catalyst separately.
