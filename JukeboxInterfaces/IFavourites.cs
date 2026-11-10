namespace JukeboxInterfaces;

public interface IFavourites
{
    int Count { get; }
    int MaxFavourites { get; }
    void RecordPlay(string fullPath);
    void Remove(string fullPath);
    IReadOnlyList<FavouriteEntry> GetTopFavourites();
    FavouriteEntry? GetRandomFavourite();

    // Removes every favourite that has the lowest play count, and returns what was removed.
    IReadOnlyList<FavouriteEntry> TrimLeastPlayed();
}

public record FavouriteEntry(string FullPath, int PlayCount);
