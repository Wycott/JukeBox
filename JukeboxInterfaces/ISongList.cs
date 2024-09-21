namespace JukeboxInterfaces;

public interface ISongList
{
    List<ISong> SongCollection { get; set; }
    void Build(ISongSources sources, string selectedPattern);
}