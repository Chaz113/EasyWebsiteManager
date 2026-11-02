using System.Text.Json;
using EasyWebsiteManager.Models;

namespace EasyWebsiteManager.Services;

public static class StorageService
{
    private const string FileName = "EasyWebsiteManager.json";
    private static readonly SemaphoreSlim StorageGate = new(1, 1);

    private static string FilePath =>
        Path.Combine(FileSystem.AppDataDirectory, FileName);

    private static string BackupPath => FilePath + ".bak";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static async Task ExportAsync(AppData data, string destinationPath)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await File.WriteAllTextAsync(destinationPath, json);
    }

    public static async Task SaveAsync(AppData data)
    {
        await StorageGate.WaitAsync();
        string? temporaryPath = null;
        try
        {
            // Serialize inside the gate so saves cannot overtake one another.
            var json = JsonSerializer.Serialize(data, JsonOptions);
            Directory.CreateDirectory(FileSystem.AppDataDirectory);
            temporaryPath = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            await File.WriteAllTextAsync(temporaryPath, json);

            // Never replace the recovery copy with an invalid primary file.
            if (File.Exists(FilePath) && await IsValidDataFileAsync(FilePath))
                File.Copy(FilePath, BackupPath, overwrite: true);

            // Replace only after the entire new file has been written.
            File.Move(temporaryPath, FilePath, overwrite: true);
            temporaryPath = null;
        }
        finally
        {
            if (temporaryPath != null)
            {
                try { File.Delete(temporaryPath); }
                catch (IOException) { /* Best-effort temporary-file cleanup. */ }
                catch (UnauthorizedAccessException) { /* Best-effort cleanup. */ }
            }
            StorageGate.Release();
        }
    }

    public static async Task<AppData?> LoadAsync()
    {
        await StorageGate.WaitAsync();
        try
        {
            // A valid primary file always takes precedence over an older backup.
            var current = await TryReadDataAsync(FilePath);
            if (current != null)
                return current;

            // Recovery is read-only: preserve the damaged file for investigation.
            return await TryReadDataAsync(BackupPath);
        }
        finally
        {
            StorageGate.Release();
        }
    }

    private static async Task<bool> IsValidDataFileAsync(string path) =>
        await TryReadDataAsync(path) != null;

    private static async Task<AppData?> TryReadDataAsync(string path)
    {
        if (!File.Exists(path))
            return null;

        try
        {
            var json = await File.ReadAllTextAsync(path);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var data = JsonSerializer.Deserialize<AppData>(json, JsonOptions);
            return data?.Categories != null && data.Settings != null ? data : null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    public static bool DataFileExists() =>
        File.Exists(FilePath) || File.Exists(BackupPath);
}
