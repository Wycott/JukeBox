using JukeboxDomain.Helpers;
using JukeboxInterfaces;

namespace JukeboxDomain;

public class Favourites : IFavourites
{
    private readonly string filePath;
    private readonly Dictionary<string, int> playCounts;

    public Favourites(string filePath, int maxFavourites = 100)
    {
        this.filePath = filePath;
        MaxFavourites = maxFavourites;
        playCounts = JsonFile.Load<Dictionary<string, int>>(filePath);
    }

    public int Count => playCounts.Count;

    // A soft limit: going over it is allowed, and the user trims with :t when they choose.
    public int MaxFavourites { get; }

    public void RecordPlay(string fullPath)
    {
        playCounts[fullPath] = playCounts.GetValueOrDefault(fullPath) + 1;
        Save();
    }

    public IReadOnlyList<FavouriteEntry> TrimLeastPlayed()
    {
        if (playCounts.Count == 0)
        {
            return [];
        }

        var lowestCount = playCounts.Values.Min();
        var removed = playCounts
            .Where(kvp => kvp.Value == lowestCount)
            .Select(kvp => new FavouriteEntry(kvp.Key, kvp.Value))
            .ToList();

        foreach (var entry in removed)
        {
            playCounts.Remove(entry.FullPath);
        }

        Save();

        return removed;
    }

    public void Remove(string fullPath)
    {
        if (playCounts.Remove(fullPath))
        {
            Save();
        }
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

    private void Save()
    {
        JsonFile.Save(filePath, playCounts);
    }
}
