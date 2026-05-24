namespace JukeboxInterfaces;

public interface ISongList
{
    IReadOnlyList<ISong> SongCollection { get; }
    void Build(ISongSources sources, string selectedPattern);
}