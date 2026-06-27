namespace JukeboxInterfaces;

public interface IBanList
{
    void Ban(string fullPath);
    bool IsBanned(string fullPath);
    string? IsSimilarToBanned(string fullPath);
    IReadOnlyList<string> GetBannedSongs();
}
