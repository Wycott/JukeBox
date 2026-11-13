using JukeboxDomain.Helpers;
using JukeboxInterfaces;

namespace JukeboxDomain;

public class Favourites : IFavourites
{
    private readonly string filePath;
    private readonly Dictionary<string, int> playCounts;
    private readonly HashSet<string> pickedThisCycle = [];
    private string? lastPicked;

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
        pickedThisCycle.Remove(fullPath);

        if (playCounts.Remove(fullPath))
        {
            Save();
        }
    }

    public IReadOnlyList<FavouriteEntry> GetTopFavourites()
    {
        // Ties are broken by path so ranks are stable between calls.
        return playCounts
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
            .Select((kvp, index) => new FavouriteEntry(kvp.Key, kvp.Value, index + 1))
            .ToList();
    }

    public int? GetRank(string fullPath)
    {
        return GetTopFavourites().FirstOrDefault(e => e.FullPath == fullPath)?.Rank;
    }

    public FavouriteEntry? GetRandomFavourite()
    {
        var ranked = GetTopFavourites();

        if (ranked.Count == 0)
        {
            return null;
        }

        var candidates = ranked.Where(e => !pickedThisCycle.Contains(e.FullPath)).ToList();

        if (candidates.Count == 0)
        {
            pickedThisCycle.Clear();

            // Don't let the last song of one cycle also be the first of the next.
            candidates = ranked.Where(e => e.FullPath != lastPicked).ToList();

            if (candidates.Count == 0)
            {
                candidates = ranked.ToList();
            }
        }

        var picked = candidates[Random.Shared.Next(candidates.Count)];
        pickedThisCycle.Add(picked.FullPath);
        lastPicked = picked.FullPath;

        return picked;
    }

    private void Save()
    {
        JsonFile.Save(filePath, playCounts);
    }
}
