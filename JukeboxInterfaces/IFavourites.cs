namespace JukeboxInterfaces;

public interface IFavourites
{
    int Count { get; }
    int MaxFavourites { get; }
    void RecordPlay(string fullPath);
    void Remove(string fullPath);
    // Ordered by play count (most played first); each entry's Rank is its 1-based position.
    IReadOnlyList<FavouriteEntry> GetTopFavourites();

    // Picks a random favourite not yet picked in the current cycle; once every favourite
    // has been picked, a new cycle starts.
    FavouriteEntry? GetRandomFavourite();

    // Removes every favourite that has the lowest play count, and returns what was removed.
    IReadOnlyList<FavouriteEntry> TrimLeastPlayed();
}

public record FavouriteEntry(string FullPath, int PlayCount, int Rank = 0);
