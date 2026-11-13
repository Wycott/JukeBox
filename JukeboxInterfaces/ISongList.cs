namespace JukeboxInterfaces;

public interface ISongList
{
    IReadOnlyList<ISong> SongCollection { get; }
    void Build(ISongSources sources, string selectedPattern);

    // Builds a list of every (non-banned) song in the sources, in random order.
    void BuildShuffled(ISongSources sources);
}