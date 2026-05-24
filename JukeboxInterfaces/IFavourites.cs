namespace JukeboxInterfaces;

public interface IFavourites
{
    void RecordPlay(string fullPath);
    IReadOnlyList<FavouriteEntry> GetTopFavourites();
    FavouriteEntry? GetRandomFavourite();
}

public record FavouriteEntry(string FullPath, int PlayCount);
