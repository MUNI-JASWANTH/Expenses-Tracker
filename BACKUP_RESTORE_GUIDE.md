# Backup & Restore (new feature)

- **Export backup:** Open the Backup tab and tap **Export backup**. Use the operating system share/save sheet to save the generated `.json` file in a safe place. A file created in app cache is not a durable backup until you save/share it elsewhere.
- **Restore backup:** Open the Backup tab, select the saved JSON file, and explicitly confirm replacement. This replaces *all* local transactions and budgets. Navigate away and back to other tabs to refresh their data.
- The backup contains transaction descriptions, amounts, dates, categories, and budgets in plain-text JSON. **Not encrypted**: keep files private. It is not automatic cloud backup.
- Restore validates version, record counts, record IDs, category/type, amounts, and date ranges before modifying the database. Database replacement is performed within a SQLite transaction.
- Backup format version: 1. Only this application's exported backup format is supported.
- The app remains local-first. No online account or synchronization is included.

## Manual acceptance tests (Windows first, then Android, iOS and Mac Catalyst)

1. Add one income, two expenses and a budget. Export a backup and save it outside the app.
2. Edit or delete an entry. Restore the backup and verify that the original entries and budget return.
3. Cancel file selection and cancel confirmation: data must not change.
4. Import invalid JSON, an unsupported version and a file over 20 MB: import must fail without deleting data.
5. Check the dashboard, transaction list, budgets and reports after navigating away and back.
6. Repeat on each platform, including sharing/saving and picking a file.

**Verification:** XML and package integrity are checked during packaging; no MAUI SDK or platform runtimes are available in the build environment, so compilation and end-to-end behavior still need validation in Visual Studio and on devices.
