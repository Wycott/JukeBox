namespace JukeboxLibrary.Interfaces;

public interface ISongList
{
    List<ISong> SongCollection { get; set; }
    void Build(ISongSources sources, string selectedPattern);
}