using System.Text.Json;
using JukeboxInterfaces;

namespace JukeboxDomain;

public class Favourites : IFavourites
{
    private readonly int _maxFavourites;
    private readonly string _filePath;
    private Dictionary<string, int> _playCounts;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public Favourites(string filePath, int maxFavourites = 100)
    {
        _filePath = filePath;
        _maxFavourites = maxFavourites;
        _playCounts = Load();
    }

    public void RecordPlay(string fullPath)
    {
        if (_playCounts.ContainsKey(fullPath))
        {
            _playCounts[fullPath]++;
        }
        else
        {
            _playCounts[fullPath] = 1;
        }

        Trim();
        Save();
    }

    public IReadOnlyList<FavouriteEntry> GetTopFavourites()
    {
        return _playCounts
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => new FavouriteEntry(kvp.Key, kvp.Value))
            .ToList();
    }

    public FavouriteEntry? GetRandomFavourite()
    {
        if (_playCounts.Count == 0)
        {
            return null;
        }

        var entries = _playCounts.Keys.ToList();
        var index = Random.Shared.Next(entries.Count);
        var key = entries[index];

        return new FavouriteEntry(key, _playCounts[key]);
    }

    private void Trim()
    {
        if (_playCounts.Count <= _maxFavourites)
        {
            return;
        }

        _playCounts = _playCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(_maxFavourites)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_playCounts, JsonOptions);
            File.WriteAllText(_filePath, json);
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
            if (!File.Exists(_filePath))
            {
                return new Dictionary<string, int>();
            }

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            return new Dictionary<string, int>();
        }
    }
}
