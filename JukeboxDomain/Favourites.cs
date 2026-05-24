using System.Text.Json;
using JukeboxInterfaces;

namespace JukeboxDomain;

public class Favourites : IFavourites
{
    private readonly int maxFavourites;
    private readonly string filePath;
    private Dictionary<string, int> playCounts;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public Favourites(string filePath, int maxFavourites = 100)
    {
        this.filePath = filePath;
        this.maxFavourites = maxFavourites;
        playCounts = Load();
    }

    public void RecordPlay(string fullPath)
    {
        if (playCounts.ContainsKey(fullPath))
        {
            playCounts[fullPath]++;
        }
        else
        {
            playCounts[fullPath] = 1;
        }

        Trim();
        Save();
    }

    public IReadOnlyList<FavouriteEntry> GetTopFavourites()
    {
        return playCounts
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => new FavouriteEntry(kvp.Key, kvp.Value))
            .ToList();
    }

    public FavouriteEntry? GetRandomFavourite()
    {
        if (playCounts.Count == 0)
        {
            return null;
        }

        var entries = playCounts.Keys.ToList();
        var index = Random.Shared.Next(entries.Count);
        var key = entries[index];

        return new FavouriteEntry(key, playCounts[key]);
    }

    private void Trim()
    {
        if (playCounts.Count <= maxFavourites)
        {
            return;
        }

        playCounts = playCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(maxFavourites)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(playCounts, JsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch (IOException)
        {
            // Best effort — don't crash if we can't write
        }
    }

    private Dictionary<string, int> Load()
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new Dictionary<string, int>();
            }

            var json = File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            return new Dictionary<string, int>();
        }
    }
}
