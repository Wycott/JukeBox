using JukeboxDomain.Helpers;
using JukeboxInterfaces;

namespace JukeboxDomain;

public class Favourites : IFavourites
{
    private readonly int maxFavourites;
    private readonly string filePath;
    private readonly Dictionary<string, int> playCounts;

    public Favourites(string filePath, int maxFavourites = 100)
    {
        this.filePath = filePath;
        this.maxFavourites = maxFavourites;
        playCounts = JsonFile.Load<Dictionary<string, int>>(filePath);
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

        Trim(keep: fullPath);
        Save();
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

    private void Trim(string keep)
    {
        // Evict the least-played entries, but never the song that was just played -
        // otherwise a new song (count 1) is always the one evicted and a full list can never change.
        while (playCounts.Count > maxFavourites)
        {
            var candidates = playCounts.Where(kvp => kvp.Key != keep).ToList();

            if (candidates.Count == 0)
            {
                break;
            }

            var victim = candidates.MinBy(kvp => kvp.Value).Key;
            playCounts.Remove(victim);
        }
    }

    private void Save()
    {
        JsonFile.Save(filePath, playCounts);
    }
}
