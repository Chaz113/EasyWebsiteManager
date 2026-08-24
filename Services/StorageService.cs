using System.Text.Json;
using EasyWebsiteManager.Models;

namespace EasyWebsiteManager.Services;

public static class StorageService
{
    private const string FileName = "EasyWebsiteManager.json";

    private static string FilePath =>
        Path.Combine(FileSystem.AppDataDirectory, FileName);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static async Task SaveAsync(AppData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);

        await File.WriteAllTextAsync(
            FilePath,
            json);
    }

    public static async Task<AppData?> LoadAsync()
    {
        if (!File.Exists(FilePath))
            return null;

        var json = await File.ReadAllTextAsync(FilePath);

        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonSerializer.Deserialize<AppData>(
            json,
            JsonOptions);
    }

    public static bool DataFileExists()
    {
        return File.Exists(FilePath);
    }
}