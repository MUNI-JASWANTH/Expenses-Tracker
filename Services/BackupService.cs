using System.Text.Json;
using FirstMauiApp.Models;
using SQLite;

namespace FirstMauiApp.Services;

/// <summary>Portable JSON backup for transactions and budgets. Imports replace all existing records.</summary>
public sealed class BackupService
{
    private const int CurrentVersion = 1;
    private const long MaxBackupBytes = 20 * 1024 * 1024;
    private readonly DatabaseService _database;
    private readonly SemaphoreSlim _operationLock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public BackupService(DatabaseService database) => _database = database;

    public async Task<string> CreateBackupAsync()
    {
        await _operationLock.WaitAsync();
        try
        {
            var (transactions, budgets) = await _database.ReadBackupDataAsync();
            var document = new BackupDocument(CurrentVersion, DateTimeOffset.UtcNow, transactions, budgets);
            var directory = Path.Combine(FileSystem.CacheDirectory, "Backups");
            Directory.CreateDirectory(directory);
            var file = Path.Combine(directory, $"expense-backup-{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.json");
            await File.WriteAllTextAsync(file, JsonSerializer.Serialize(document, JsonOptions));
            return file;
        }
        finally { _operationLock.Release(); }
    }

    public async Task<(int Transactions, int Budgets)> RestoreBackupAsync(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        await _operationLock.WaitAsync();
        try
        {
            using var buffer = new MemoryStream();
            var chunk = new byte[81920];
            int read;
            while ((read = await source.ReadAsync(chunk)) != 0)
            {
                if (buffer.Length + read > MaxBackupBytes)
                    throw new InvalidDataException("Backup exceeds the 20 MB safety limit.");
                buffer.Write(chunk, 0, read);
            }
            buffer.Position = 0;
            BackupDocument? backup;
            try { backup = await JsonSerializer.DeserializeAsync<BackupDocument>(buffer, JsonOptions); }
            catch (JsonException ex) { throw new InvalidDataException("This is not a valid expense tracker backup.", ex); }
            Validate(backup);
            await _database.ReplaceBackupDataAsync(backup!.Transactions, backup.Budgets);
            return (backup.Transactions.Count, backup.Budgets.Count);
        }
        finally { _operationLock.Release(); }
    }

    private static void Validate(BackupDocument? backup)
    {
        if (backup is null || backup.Version != CurrentVersion || backup.Transactions is null || backup.Budgets is null)
            throw new InvalidDataException("Unsupported or incomplete backup file.");
        if (backup.Transactions.Count > 100000 || backup.Budgets.Count > 100000)
            throw new InvalidDataException("Backup contains too many records.");
        var transactionIds = new HashSet<int>();
        foreach (var t in backup.Transactions)
        {
            if (t is null || t.Id <= 0 || !transactionIds.Add(t.Id) || t.Amount <= 0 ||
                t.Date.Year is < 1 or > 9998 || string.IsNullOrWhiteSpace(t.Description) || t.Description.Length > 2000 ||
                !Enum.IsDefined(t.Type) || !Enum.IsDefined(t.Category) ||
                (t.Type == TransactionType.Income ? t.Category > TransactionCategory.OtherIncome : t.Category < TransactionCategory.Food))
                throw new InvalidDataException("Backup contains an invalid transaction.");
        }
        var budgetIds = new HashSet<int>();
        var budgetKeys = new HashSet<(int, int, TransactionCategory)>();
        foreach (var b in backup.Budgets)
        {
            if (b is null || b.Id <= 0 || !budgetIds.Add(b.Id) || b.Month is < 1 or > 12 ||
                b.Year is < 1 or > 9999 || b.MonthlyLimit <= 0 || !Enum.IsDefined(b.Category) ||
                b.Category < TransactionCategory.Food || !budgetKeys.Add((b.Year, b.Month, b.Category)))
                throw new InvalidDataException("Backup contains an invalid budget.");
        }
    }

    public sealed record BackupDocument(int Version, DateTimeOffset ExportedAtUtc,
        List<Transaction> Transactions, List<Budget> Budgets);
}
